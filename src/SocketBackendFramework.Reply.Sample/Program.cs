using SocketBackendFramework.Reply.Workflows;
using SocketBackendFramework.Reply.Models.Workflows;
using SocketBackendFramework.Reply.Sample.Models;
using SocketBackendFramework.Reply.Sample.Workflows;
using System.Text.Json;
using System.Text.Json.Serialization;

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

WorkflowPoolConfig workflowPoolConfig = configRoot.WorkflowPoolConfig;

WorkflowPool workflowPool = new(workflowPoolConfig);

WorkflowConfig workflowConfig = workflowPool.GetWorkflowConfig("default");

IWorkflowBuilder workflowBuilder = new DefaultWorkflowBuilder(workflowConfig);

workflowPool.AddWorkflow(workflowBuilder.Build());

workflowPool.Start();
Console.WriteLine("Started");

// wait forever
await Task.Delay(-1);
