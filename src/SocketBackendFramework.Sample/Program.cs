using SocketBackendFramework.Workflows;
using SocketBackendFramework.Models.Workflows;
using SocketBackendFramework.Sample.Models;
using SocketBackendFramework.Sample.Workflows;
using SocketBackendFramework.Middlewares;
using System.Text.Json;
using System.Text.Json.Serialization;
using SocketBackendFramework.Sample.Helpers;

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

PipelineBuilder pipelineBuilder = new();

WorkflowBuilder workflowBuilder = new DefaultWorkflowBuilder(workflowConfig,
                                                             pipelineBuilder,
                                                             new ContextAdaptor());

workflowPool.AddWorkflow(workflowBuilder);

workflowPool.Start();

// wait forever
await Task.Delay(-1);
