using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Text;
using FoundationaLLM.Common.Services.TextSplitters;
using FoundationaLLM.Common.Services.Tokenizers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Common.Tests.Services.TextSplitters
{
    public class TokenTextSplitterServiceTests
    {
        [Fact]
        public void TestSplitPlainText()
        {
            ITokenizerService tokenizerService = new MicrosoftBPETokenizerService(
                LoggerFactory
                    .Create(builder => builder.AddConsole())
                    .CreateLogger<MicrosoftBPETokenizerService>()
            );

            // Only TokenizerEncoder used -- no need to specify the Tokenizer name
            IOptions<TokenTextSplitterServiceSettings> options = Options.Create(new TokenTextSplitterServiceSettings("", TikTokenizerEncoders.CL100K_BASE, 5, 4));

            TokenTextSplitterService tokenTextSplitterService = new TokenTextSplitterService(
                tokenizerService,
                options,
                LoggerFactory
                    .Create(builder => builder.AddConsole())
                    .CreateLogger<TokenTextSplitterService>()
            );

            var splitTextResult = tokenTextSplitterService.SplitPlainText("Some Word or Phrase With Seven Tokens");

            // Initially, there should be [(7 - 4) / (5 - 4)] = 3 chunks
            // However, the last two chunks should be merged
            Assert.Equal(2, splitTextResult.Count);
            Assert.Equal(5, splitTextResult[0].Content!.Split(" ").Count(word => !string.IsNullOrWhiteSpace(word)));
            Assert.Equal(6, splitTextResult[1].Content!.Split(" ").Count(word => !string.IsNullOrWhiteSpace(word)));

            // Do not merge the last two chunks with this test case
            options = Options.Create(new TokenTextSplitterServiceSettings("", TikTokenizerEncoders.CL100K_BASE, 3, 2));
            tokenTextSplitterService = new TokenTextSplitterService(
                tokenizerService,
                options,
                LoggerFactory
                    .Create(builder => builder.AddConsole())
                    .CreateLogger<TokenTextSplitterService>()
            );
            splitTextResult = tokenTextSplitterService.SplitPlainText("Some Word or Phrase With Seven Tokens");
            Assert.Equal(5, splitTextResult.Count);
            // Excluding whitespace, there should be three words in each output chunk
            Assert.Equal(
                -1,
                splitTextResult.FindIndex(chunk => chunk.Content!.Split(" ").Count(word => !string.IsNullOrWhiteSpace(word)) != 3)
            );
        }
    }
}
