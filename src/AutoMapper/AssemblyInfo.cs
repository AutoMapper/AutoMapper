using System;
using System.Security;

[assembly:CLSCompliant(true)]
[assembly: AllowPartiallyTrustedCallers]
[assembly: SecurityTransparent]
#if NETSTANDARD1_3
[assembly: SecurityRules(SecurityRuleSet.Level2, SkipVerificationInFullTrust = true)]
#endif