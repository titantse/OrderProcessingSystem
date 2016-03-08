
namespace OrderProcessing.Core
{
    using System;
    using System.Collections.Generic;
    using OrderProcessing.Common;
    using OrderProcessing.Domain;
    using OrderProcessing.Logger;
    /// <summary>
    /// A simple state machine to represent the status transffering.
    /// </summary>
    public class OrderStateMachine
    {
        /// <summary>
        /// The count of OrderStatus.
        /// </summary>
        private const int ORDER_STATUS_COUT = 7;

        /// <summary>
        /// A map for status to an action
        /// means should do the action if the OrderProcessingInfo's status is mapped one.
        /// </summary>
        private readonly Dictionary<OrderStatus, Func<OrderProcessingInfo, ProcessorStepResult>> actions =
            new Dictionary<OrderStatus, Func<OrderProcessingInfo, ProcessorStepResult>>();

        /// <summary>
        /// The FSM of order-processing, stored the current staus and next status according the process step result(success, failed)
        /// </summary>
        private readonly Dictionary<OrderStatus, Dictionary<bool, OrderStatus>> stateMachine =
            new Dictionary<OrderStatus, Dictionary<bool, OrderStatus>>();

        /// <summary>
        /// The list of status
        /// </summary>
        private readonly List<OrderStatus> statuses = new List<OrderStatus>();

        /// <summary>
        /// Initialize the data in constructor data.
        /// </summary>
        public OrderStateMachine()
        {
            for (var i = 0; i < ORDER_STATUS_COUT; ++i)
            {
                statuses.Add((OrderStatus)i);
            }

            var failed = OrderStatus.Failed;
            //by deafult, each status's is mapped to a method which just directly return the answer.
            foreach (var status in statuses)
            {
                //if no operation is set to an status, an empty action would be set.
                if (!actions.ContainsKey(status))
                {
                    actions[status] = info => { return new ProcessorStepResult(info, true); };
                }
            }
            //The FSM here is very simple, for every status before PostPrecessing, 
            //if the step processing goes well,
            //the status goes to next status( (OrderStatus)(status + 1)) until Sunday
            //if the step failed,
            //the status goes to Failed.
            for (var i = 0; i < ORDER_STATUS_COUT - 2; ++i)
            {
                var nextStatus = new Dictionary<bool, OrderStatus>
                {
                    {true, statuses[i + 1]},
                    {false, failed}
                };
                stateMachine[statuses[i]] = nextStatus;
            }
        }

        /// <summary>
        /// Set the status-action mapping.
        /// If the method been called twice with same status, the action would be the latter one.
        /// </summary>
        /// <param name="status"></param>
        /// <param name="action"></param>
        public void SetOperation(OrderStatus status, Func<OrderProcessingInfo, ProcessorStepResult> action)
        {
            actions[status] = action;
        }


        /// <summary>
        ///     Start to run the state machine from the order's current status.
        ///     If program crashes at any step, in each step, execute result(success/fail) is recored.
        ///     When the order been re-processed, it would start from next status against last execution result.
        /// </summary>
        /// <param name="orderInfo"></param>
        /// <returns></returns>
        public OrderProcessingInfo Run(OrderProcessingInfo orderInfo)
        {
            var currentStatus = orderInfo.Status;
            //if the status is already the final status, then skip.
            if (!stateMachine.ContainsKey(currentStatus)) return orderInfo;
            var lastStepSuccess = true;
            //if the order is been recovered, then it's stepsInfo should not be empty
            //then the lastStepSuccess would be set as last processed result.
            if (orderInfo.StepsInfo != null && orderInfo.StepsInfo.ContainsKey(currentStatus))
                lastStepSuccess = orderInfo.StepsInfo[currentStatus].Success;
            //start from next status.
            currentStatus = stateMachine[currentStatus][lastStepSuccess];
            orderInfo.Status = currentStatus;
            while (stateMachine.ContainsKey(currentStatus))
            {
                Logger.LogInformation("Order {0} processing with status {1} start...".FormatWith(orderInfo.Id,
                    currentStatus));
                var stepResult = new ProcessorStepResult(orderInfo, false);
                try
                {
                    stepResult = actions[currentStatus].Invoke(orderInfo);
                }
                catch (Exception ex)
                {
                    if (ex is OrderProcessingException &&
                        ((OrderProcessingException)ex).ErrorCode.Code == ErrorCode.TimestampConflict.Code)
                    {
                        //other worknode has picked it up, should just abort the state machine.
                        Logger.LogWarning("Order {0} has been picked up by other node.".FormatWith(orderInfo.Id));
                        throw ex;
                    }
                    Logger.LogException(ex,
                        "Order {0} Process faild with status {1}".FormatWith(orderInfo.Id, currentStatus));
                    stepResult.Success = false;
                }
                Logger.LogInformation(
                    "Order {0} processing with status {1} finished with success:{2}".FormatWith(orderInfo.Id,
                        currentStatus, stepResult.Success));
                //currentStatus -> nextStatus
                currentStatus = stateMachine[currentStatus][stepResult.Success];
                orderInfo = stepResult.ProcessingInfo;
                orderInfo.Status = currentStatus;
            }
            //If failure happens here, the order would be re-picked up in the future.
            if (actions.ContainsKey(currentStatus))
            {
                //If error happens at last state, just wll let the order timed out.
                var stepResult = actions[currentStatus].Invoke(orderInfo);
                orderInfo = stepResult.ProcessingInfo;
                orderInfo.Status = currentStatus;
            }
            return orderInfo;
        }
    }
}