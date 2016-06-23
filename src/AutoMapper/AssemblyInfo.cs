using System;
using System.Security;

[assembly:CLSCompliant(true)]
#if NETSTANDARD1_3
[assembly: AllowPartiallyTrustedCallers]
[assembly: SecurityTransparent]
[assembly: SecurityRules(SecurityRuleSet.Level2, SkipVerificationInFullTrust = true)]
#endif