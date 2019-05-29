using System.Security;
using BenchmarkDotNet.Running;

//[assembly: AllowPartiallyTrustedCallers]
//[assembly: SecurityTransparent]
//[assembly: SecurityRules(SecurityRuleSet.Level2, SkipVerificationInFullTrust = true)]

namespace Benchmark
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run(typeof(Program).Assembly);
        }
    }
}
