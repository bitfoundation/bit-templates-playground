namespace System;

public static class GuidExtensions
{
    extension(Guid source)
    {
        /// <summary>
        /// Generates a sequential GUID optimized for database primary key usage.
        /// 
        /// <para>
        /// <b>Warning</b>
        /// Both PostgreSQL and SQL Server are configured to natively generate sequential identifiers 
        /// if the entity's ID is not provided. Use this method only when application-side GUID key generation is required.
        /// </para>
        /// 
        /// <para>
        /// <b>Benefit of Sequential GUIDs as Clustered Indexes</b>
        /// Even with NVMe SSDs, random GUIDs can degrade performance due to frequent Page Splits; 
        /// therefore, sequential identifiers are preferred.
        /// </para>
        /// 
        /// <para>
        /// <b>Guid.CreateVersion7() vs CreateSequentialGuid()</b>
        /// While <see cref="Guid.CreateVersion7()"/> 
        /// provides time-ordered GUIDs for most engines, SQL Server employs a unique sorting logic 
        /// that necessitates byte reordering to maintain physical sequentiality.
        /// The <see cref="CreateSequentialGuid()"/> method implements this SQL Server-specific byte rearrangement.
        /// </para>
        /// 
        /// <para>
        /// <b>Handling Offline/Sync Scenarios</b>
        /// In offline-capable applications, using application-side GUIDs is useful when inserting parent and child records before syncing them to the server. 
        /// But regardless of sequentiality, "Late-Arriving Data" (records arriving out of chronological order relative to the server) will inevitably cause index fragmentation.
        /// For high-scale offline scenarios, it is recommended to use a server-generated <c>ServerCreatedAtUtc</c> 
        /// column as the Clustered Index and move the GUID <c>Id</c> and <c>IsArchived</c> to a Non-Clustered filtered index.
        /// </para>
        /// </summary>
        public static Guid CreateSequentialGuid()
        {
            Guid standardV7 = Guid.CreateVersion7();

            return standardV7;
            // SQL Server specific byte rearrangement is not needed for your chosen database engine.
        }
    }
}
