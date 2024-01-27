using System.Collections.Immutable;
using Pulumi;

namespace Ict.PulumiBuildingBlocks.Extensions
{
    /// <summary>
    /// Provides extension methods to simplify common operations on <see cref="InputList{T}"/>
    /// </summary>
    public static class InputListExtensions
    {
        /// <summary>
        /// Converts an InputList&lt;string&gt; to InputList&lt;Union&lt;string, T&gt;&gt;.
        /// 
        /// In Pulumi a number of properties are defined as Union&lt;string, T&gt; but there is no conversion
        /// mechanism from <see cref="string"/> or <typeparamref name="T"/> to a Union. This method provides this
        /// capability. 
        /// </summary>
        /// <typeparam name="T">The strong-type enumeration appearing as a second type on a Pulumi Union&lt;string, T&gt;.</typeparam>
        /// <param name="input">The InputList&lt;string&gt; to convert.</param>
        /// <returns>The InputList&lt;Union&lt;string, T&gt;&gt; created from the input.</returns>
        public static InputList<Union<string, T>> AsUnion<T>(this InputList<string> input) where T : struct
            => input.Apply(inputArray => inputArray.Select(Union<string, T>.FromT0));

        /// <summary>
        /// Converts an InputList&lt;T&gt; to InputList&lt;Union&lt;string, T&gt;&gt;.
        /// 
        /// In Pulumi a number of properties are defined as Union&lt;string, T&gt; but there is no conversion
        /// mechanism from <see cref="string"/> or <typeparamref name="T"/> to a Union. This method provides this
        /// capability. 
        /// </summary>
        /// <typeparam name="T">The strong-type enumeration appearing as a second type on a Pulumi Union&lt;string, T&gt;.</typeparam>
        /// <param name="input">The InputList&lt;T&gt; to convert.</param>
        /// <returns>The InputList&lt;Union&lt;string, T&gt;&gt; created from the input.</returns>
        public static InputList<Union<string, T>> AsUnion<T>(this InputList<T> input) where T : struct
            => input.Apply(inputArray => inputArray.Select(Union<string, T>.FromT1));

        /// <summary>
        /// Converts an InputList&lt;T&gt; to InputList&lt;Union&lt;string, T&gt;&gt;. When <paramref name="input"/>
        /// is <c>null</c> or empty the <paramref name="defaultValue"/> will be used.
        /// 
        /// In Pulumi a number of properties are defined as Union&lt;string, T&gt; but there is no conversion
        /// mechanism from <see cref="string"/> or <typeparamref name="T"/> to a Union. This method provides this
        /// capability. 
        /// </summary>
        /// <typeparam name="T">The strong-type enumeration appearing as a second type on a Pulumi Union&lt;string, T&gt;.</typeparam>
        /// <param name="input">The InputList&lt;T&gt; to convert.</param>
        /// <param name="defaultValue">The value to be used when <paramref name="input"/> is <c>null</c> or empty.</param>
        /// <returns>The InputList&lt;Union&lt;string, T&gt;&gt; created from the input.</returns>
        /// <remarks>
        /// The default value is set at this stage instead on the relevant resource argument class because the <see cref="InputList{T}"/>
        /// exhibits a behaviour that makes it unreliable as, depending on how the end user initialises the collection, we can end up
        /// with the default value and any other value added afterwards. See https://github.com/pulumi/pulumi/issues/8048.
        /// </remarks>
        public static InputList<Union<string, T>> AsUnion<T>(this InputList<T>? input, T defaultValue) where T : struct
        {
            var defaultArrayValue = ImmutableArray.Create<Union<string, T>>(defaultValue);
            return input == null
                ? Output.Create(defaultArrayValue)
                : input.Apply(inputArray =>
                    inputArray.Length == 0 ? defaultArrayValue : inputArray.Select(Union<string, T>.FromT1).ToImmutableArray());
        }
    }
}