
namespace OrderProcessing.DataAccessor
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using Newtonsoft.Json;
    using OrderProcessing.Common;
    using OrderProcessing.Domain;
    using OrderProcessing.Domain.Request;
    using OrderProcessing.Interface;
    /// <summary>
    ///     DataAccessor is the implementation of IOrderRepository and INodeMonitor.
    ///     By default, the implementation is on top of SQL Server.
    ///     orderRepositoryOverrided and nodeMonitorOverrided can be set for testing purpose.
    /// </summary>
    public class DataAccessor : IOrderRepository, INodeMonitor
    {
        private static IOrderRepository orderRepositoryOverrided;
        private static INodeMonitor nodeMonitorOverrided;
        private readonly string _connectionStr;

        public static IOrderRepository OrderRepository
        {
            get { return orderRepositoryOverrided ?? Instance; }
            set { orderRepositoryOverrided = value; }
        }

        public static INodeMonitor NodeMonitor
        {
            get { return nodeMonitorOverrided ?? Instance; }
            set { nodeMonitorOverrided = value; }
        }

        public void ReportNodeStarted(string nodeId)
        {
            QueryStoreProcedure("sp_report_node_start", new Dictionary<string, object>
            {
                {"@work_node_id", nodeId}
            });
        }

        public void ReportNodeHeartBeat(string nodeId, int queueSize)
        {
            QueryStoreProcedure("sp_report_node_heart_beat", new Dictionary<string, object>
            {
                {"@work_node_id", nodeId},
                {"@processing_order_count", queueSize}
            });
        }

        public OrderProcessingInfo GetOrderProcessingInfoByTrackingId(string Id)
        {
            var result = QueryStoreProcedure("sp_get_processing_info_by_tracking_id",
                new Dictionary<string, object> { { "@tracking_id", Id } });
            return result.Tables[0].Rows.Count == 0 ? null : new OrderProcessingInfo().FromRow(result.Tables[0].Rows[0]);
        }

        public OrderProcessingInfo GetOrderProcessingInfoById(string Id)
        {
            var result = QueryStoreProcedure("sp_get_processing_info_by_id",
                new Dictionary<string, object> { { "@id", Id } });
            return result.Tables[0].Rows.Count == 0 ? null : new OrderProcessingInfo().FromRow(result.Tables[0].Rows[0]);
        }

        public List<OrderProcessingInfo> GetNewProcessingInfos(int count, string nodeId)
        {
            var result = QueryStoreProcedure("sp_get_new_processing_infos", new Dictionary<string, object>
            {
                {"@count", count},
                {"@processing_node_id", nodeId}
            });
            return convertOrderProcessingInfos(result);
        }

        public List<OrderProcessingInfo> GetDeadNodesProcessingInfos(int count, string nodeId, int maxNoHeartBeatSeconds)
        {
            var result = QueryStoreProcedure("[sp_get_dead_nodes_processing_infos]", new Dictionary<string, object>
            {
                {"@count", count},
                {"@processing_node_id", nodeId},
                {"@max_no_heart_beat_seconds", maxNoHeartBeatSeconds}
            });
            return convertOrderProcessingInfos(result);
        }

        public List<OrderProcessingInfo> GetTimedOutProcessingInfos(int count, string nodeId, int timedOutSeconds)
        {
            var result = QueryStoreProcedure("sp_get_timedout_processing_infos", new Dictionary<string, object>
            {
                {"@count", count},
                {"@processing_node_id", nodeId},
                {"@timed_out_seconds", timedOutSeconds}
            });
            return convertOrderProcessingInfos(result);
        }

        public OrderProcessingInfo UpdateProcessingInfo(OrderProcessingInfo info)
        {
            var result = QueryStoreProcedure("sp_update_processing_info", new Dictionary<string, object>
            {
                {"@id", info.Id},
                {"@detail", info.OrderDetail},
                {"@status", info.Status.ToString()},
                {"@complete_time", info.CompleteTime.HasValue ? (object) info.CompleteTime.Value : DBNull.Value},
                {"@processing_node_id", info.ProcessingNodeId},
                {"@steps_info", JsonConvert.SerializeObject(info.StepsInfo)},
                {"@start_time", info.StartTime.HasValue ? (object) info.StartTime.Value : DBNull.Value},
                {"@timestamp", Convert.FromBase64String(info.Timestamp)}
            });
            return new OrderProcessingInfo().FromRow(result.Tables[0].Rows[0]);
        }

        public OrderProcessingInfo CreateOrderProcessingInfo(OrderCreationRequest request)
        {
            var id = IDGenerator.NewOrderId();
            var result = QueryStoreProcedure("sp_create_processing_info", new Dictionary<string, object>
            {
                {"@id", id},
                {"@tracking_id", request.TrackingId},
                {"@detail", request.OrderDetail}
            });
            return new OrderProcessingInfo().FromRow(result.Tables[0].Rows[0]);
        }

        public bool Ping()
        {
            var result = QueryStoreProcedure("spop_PingDB", null);
            return result.Tables.Count > 0;
        }

        private DataSet QueryStoreProcedure(string spName, IDictionary<string, object> parameters,
            [CallerMemberName] string callerMemberName = "")
        {
            var ds = new DataSet();
            var startTime = DateTimeOffset.UtcNow;
            var success = false;
            try
            {
                using (var cnn = new SqlConnection(_connectionStr))
                using (var cmd = new SqlCommand())
                using (var adapter = new SqlDataAdapter(cmd))
                {
                    cmd.Connection = cnn;
                    cmd.CommandTimeout = 14400; // 4 hours = 4 * 3600 seconds
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = spName;
                    if (parameters != null && parameters.Count > 0)
                    {
                        cmd.Parameters.AddRange(parameters.Select(p => new SqlParameter(p.Key, p.Value)).ToArray());
                    }
                    cnn.Open();
                    adapter.Fill(ds);
                    success = true;
                }
            }
            catch (SqlException se)
            {
                Logger.Logger.LogException(
                    "Fail to query with connection string:{0}; stored procedure:{1}. Exception Detail:{2}"
                        .FormatWith(_connectionStr, spName, se),
                    callerMemberName);

                throw new OrderProcessingException(ErrorCode.MappingSqlError(se), se);
            }
            finally
            {
                Logger.Logger.LogDependency(Logger.Logger.DepKindSQL, GetType().Name, spName, startTime, success,
                    callerMemberName);
            }
            return ds;
        }

        private List<OrderProcessingInfo> convertOrderProcessingInfos(DataSet result)
        {
            var list = new List<OrderProcessingInfo>();
            if (result.Tables.Count == 0) return list;
            foreach (DataRow row in result.Tables[0].Rows)
            {
                try
                {
                    var orderProcessInfo = new OrderProcessingInfo().FromRow(row);
                    list.Add(orderProcessInfo);
                }
                catch (OrderProcessingException exception)
                {
                    Logger.Logger.LogException(exception, "Convert OrderProcessing Info");
                }
            }
            return list;
        }

        #region singleton instance

        private static DataAccessor _instance;

        private static DataAccessor Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DataAccessor();
                }
                return _instance;
            }
        }

        private DataAccessor()
        {
            _connectionStr = ConfigurationManager.ConnectionStrings["OrderDB"].ConnectionString;
        }

        #endregion
    }
}