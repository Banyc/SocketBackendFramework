using System.Text.Json;
using System.Text.Json.Serialization;
using SocketBackendFramework.Relay.Models.Workflows;
using SocketBackendFramework.Relay.Sample.Models;
using SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow;
using SocketBackendFramework.Relay.Workflows;

string configJsonString = File.ReadAllText("config.json");
var jsonSerializerOptions = new JsonSerializerOptions()
{
    ReadCommentHandling = JsonCommentHandling.Skip,
    Converters =
    {
        // allow converting string to enum
        new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
    }
};
ConfigRoot configRoot = JsonSerializer.Deserialize<ConfigRoot>(configJsonString,
                                                               jsonSerializerOptions);

WorkflowPoolConfig workflowPoolConfig = configRoot.WorkflowPool;

WorkflowPool workflowPool = new(workflowPoolConfig);

WorkflowConfig workflowConfig = workflowPool.GetWorkflowConfig("default");

WorkflowBuilder workflowBuilder = new DefaultWorkflowBuilder(workflowConfig);

workflowPool.AddWorkflow(workflowBuilder.Build());

workflowPool.Start();
Console.WriteLine("Started");

// wait forever
await Task.Delay(-1);
