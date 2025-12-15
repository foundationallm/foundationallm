# Using the Code Interpreter Tool

Leverage the Code Interpreter tool for data analysis, calculations, and dynamic code execution.

## What Is Code Interpreter?

Code Interpreter is a tool that allows agents to write and execute Python code in real-time. When enabled, agents can:

- Analyze data from uploaded files
- Perform complex calculations
- Generate charts and visualizations
- Transform and process data
- Create files for you to download

## When Code Interpreter Is Available

The Code Interpreter tool is available when:
- The agent you're using has Code Interpreter enabled
- Your administrator has configured Python execution environments
- You're asking questions that benefit from code execution

> **Note:** Not all agents have Code Interpreter. If you need this capability, check with your administrator or select an agent that supports it.

## How It Works

### The Process

1. **You ask a question** that requires computation or data analysis
2. **The agent writes Python code** to address your request
3. **Code runs in a secure environment** isolated from your system
4. **Results are returned** in the conversation as text, tables, or images

### Behind the Scenes

- Code executes in a containerized Python environment
- The agent has access to common data science libraries
- Uploaded files can be accessed by the code
- Generated files are made available for download

## What You Can Do

### Data Analysis

Upload spreadsheets or data files and ask questions:

- "What's the average sales by region in this file?"
- "Find correlations between columns A and B"
- "Show me the top 10 customers by revenue"
- "Calculate year-over-year growth rates"

### Visualizations

Request charts and graphs from your data:

- "Create a bar chart of monthly sales"
- "Plot the trend line for quarterly revenue"
- "Make a pie chart showing market share"
- "Generate a scatter plot comparing price and quantity"

### Calculations

Perform mathematical or statistical operations:

- "Calculate the compound interest on $10,000 at 5% for 10 years"
- "What's the standard deviation of these numbers?"
- "Solve this equation: 2x + 5 = 15"
- "Run a regression analysis on this dataset"

### Data Transformation

Process and transform data:

- "Convert this CSV to JSON format"
- "Clean this data by removing duplicates"
- "Merge these two datasets by customer ID"
- "Pivot this data to show totals by category"

### File Generation

Create new files from your data:

- "Export the results as a CSV file"
- "Save this chart as a PNG image"
- "Create a summary report of the analysis"
- "Generate a cleaned version of this dataset"

## Example Prompts

### Basic Analysis

> "I've uploaded a CSV file with sales data. Can you summarize the key metrics including total revenue, number of transactions, and average order value?"

### Visualization Request

> "Using the uploaded data, create a line chart showing sales trends over the past 12 months with a trend line."

### Complex Processing

> "Compare the sales performance across all regions. Show the top 3 and bottom 3 performers, and calculate the percentage difference from the average."

### Data Cleaning

> "The uploaded file has some issues — there are duplicate rows and some missing values in the 'price' column. Clean the data and show me what was fixed."

## Understanding the Output

### Text Results

The agent explains findings in natural language:
- Summaries of calculations
- Interpretations of data
- Answers to your questions

### Tables

Data results may appear as formatted tables:
- Column headers
- Organized rows of data
- Numerical results

### Charts and Images

Visualizations appear directly in the conversation:
- Click to view full size
- Right-click to save
- Charts are PNG images by default

### Generated Files

When the agent creates files:
- Download links appear in the response
- Click to download the file
- Files may be in various formats (CSV, XLSX, PNG, etc.)

## Tips for Best Results

### Provide Clear Data

- Use clean, well-formatted files
- Include clear column headers
- Remove unnecessary data before uploading
- Smaller, focused datasets process faster

### Be Specific in Requests

Instead of: "Analyze this data"
Try: "Calculate the monthly average and identify any months with values more than 2 standard deviations from the mean"

### Request Specific Formats

- Specify chart types you want
- Ask for specific file formats for exports
- Request particular calculations or metrics

### Build on Previous Results

- Reference earlier analysis in the conversation
- Ask follow-up questions
- Request modifications to previous outputs

### Check the Results

- Verify calculations make sense
- Spot-check numbers against your source data
- Ask for explanations if results are unexpected

## Limitations

### What Code Interpreter Cannot Do

- Access the internet or external systems
- Run indefinitely (there are time limits)
- Access files outside what you've uploaded
- Install arbitrary packages
- Access your local computer

### Size and Time Limits

- Very large files may take longer to process
- Complex operations may timeout
- Some requests may be too resource-intensive

### Library Availability

Common libraries are available (pandas, numpy, matplotlib, etc.) but:
- Specialized libraries may not be installed
- Ask the agent what capabilities are available

## Troubleshooting

### Code Doesn't Run

- The agent may not have Code Interpreter enabled
- Try a different agent with code execution capability
- Rephrase your request to be more specific

### Analysis Takes Too Long

- Try with a smaller sample of data
- Simplify your request
- Break complex analysis into steps

### Results Are Incorrect

- Check your source data for issues
- Be more specific about what you want
- Ask the agent to explain its approach
- Try rephrasing your question

### Chart Doesn't Generate

- Request a specific chart type
- Ensure the data is suitable for visualization
- Ask for a different visualization approach

### Can't Download Generated File

- Wait for the response to fully complete
- Click the download link in the message
- Check your browser's downloads
- Try asking the agent to regenerate the file

## Security and Privacy

- Code runs in an isolated environment
- Your uploaded data is processed by the agent
- Generated code is visible in some configurations
- Follow your organization's data policies

## Related Topics

- [Uploading Files to a Conversation](uploading-files.md) — Get data into the conversation
- [Downloading Files from a Conversation](downloading-files.md) — Save generated outputs
- [Using the Knowledge Tool](using-knowledge-tool.md) — Search document content
- [Viewing Agent Prompts](viewing-agent-prompts.md) — See how responses are generated
