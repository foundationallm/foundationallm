# Push-Location "D:\Personal\Dropbox (Personal)\Work\FoundationaLLM\LoadTesting"
Push-Location "D:\Repos\FoundationaLLM\utils\text-extraction\bin\Release\net10.0\publish\win-x64"

.\fllm-text extract `
    --input-file "D:\Personal\Dropbox (Personal)\Work\FoundationaLLM\LoadTesting\Selling-Guide_02-05-25_highlighted.pdf" `
    --output-file "D:\Personal\Dropbox (Personal)\Work\FoundationaLLM\LoadTesting\Selling-Guide_02-05-25_highlighted.txt" `
    --images

Pop-Location