using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OrderProcessing.Domain
{
    [JsonConverter(typeof (StringEnumConverter))]
    public enum OrderStatus
    {
        Pending = 0,//Order first inserted into DB.
        Scheduling = 1,//Order been pulled to the working node.
        PreProcessing = 2,
        Processing = 3,
        PostProcessing = 4,
        Compeleted = 5,
        Failed = 6//Any step above(except Completed,Failed) runs with failure or exception, then the whole Process goes to Fail.
    }
}