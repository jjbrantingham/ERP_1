// This file is kept for backward compatibility.
// IRepository has been moved to ERP.Domain.Common.Interfaces
// to fix Clean Architecture violation (Domain should not depend on Application).

[assembly: System.Runtime.CompilerServices.TypeForwardedTo(typeof(ERP.Domain.Common.Interfaces.IRepository<>))]
