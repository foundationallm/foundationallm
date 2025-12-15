# Prompt Variable

A **prompt variable** is a placeholder in a prompt that can be replaced with specific values when the prompt is executed. This allows for dynamic and flexible prompts that can adapt to different contexts or inputs.

FoundationaLLM supports prompt variables in the following format:

```plaintext
{{foundationallm:variable_name[:format]}}
```

where:

- `foundationallm` is a reserved keyword that indicates the variable is a FoundationaLLM prompt variable.
- `variable_name` is the name of the variable that will be replaced with a specific value.
- `format` is an optional parameter that specifies the format of the value to be used when replacing the variable. If not specified, the value will be used as is.

>[!NOTE]
>Prompt variables are extrapolated in a .NET module written in C# which means that `format` is a .NET format string. For more details on the available format strings, see the [Standard Numeric Format Strings](https://learn.microsoft.com/dotnet/standard/base-types/standard-numeric-format-strings) and [Standard Date and Time Format Strings](https://learn.microsoft.com/dotnet/standard/base-types/standard-date-and-time-format-strings) documentation.

The following table provides details about the prompt variables supported by FoundationaLLM:

| Variable | Description | Notes |
| --- | --- | --- |
| `{{foundationallm:router_prompt}}` | Used to insert the tool selection information into the main workflow prompt. This variable should only be used for agents that have tools associated with them. | |
| `{{foundationallm:tool_list}}` | Used to insert the list of tools available to the agent into the tool selection information (which in turn is inserted into the main workflow prompt). This variable should only be used for agents that have tools associated with them. | |
| `{{foundationallm:tool_router_prompts}}` | Used to insert additional, per tool instructions to help tool selection into the tool selection information (which in turn is inserted into the main workflow prompt). This variable should only be used for agents that have tools associated with them. | |
| `{{foundationallm:current_datetime_utc[:format]}}` | Used to insert the current date and time in UTC into the prompt. The `format` parameter is optional and can be used to specify the format of the date and time. If not specified, the default format is used. | The default format is `yyyy-MM-ddTHH:mm:ssZ`, which represents the date and time in ISO 8601 format. For more details on the available format strings, see the [Standard Date and Time Format Strings](https://learn.microsoft.com/dotnet/standard/base-types/standard-date-and-time-format-strings) documentation. |