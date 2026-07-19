// -----------------------------------------------------------------------
// <copyright file="AssemblyInfo.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

// The EF Core in-memory provider is not safe for concurrent access; running test
// classes in parallel can intermittently corrupt shared state. Disable parallelization
// so the suite is deterministic (it runs in ~1s regardless).
[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]
