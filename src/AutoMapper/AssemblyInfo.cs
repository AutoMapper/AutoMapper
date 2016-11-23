using System;

[assembly:CLSCompliant(true)]
#if NET45
using System.Security;
[assembly: AllowPartiallyTrustedCallers]
#endif
