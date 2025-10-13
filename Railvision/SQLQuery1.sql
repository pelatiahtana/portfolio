CREATE TABLE [dbo].[Incidents](
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [Type] NVARCHAR(100) NOT NULL,
    [Details] NVARCHAR(MAX) NULL,
    [Location] NVARCHAR(500) NOT NULL,
    [TrainId] NVARCHAR(50) NOT NULL,
    [OperatorId] NVARCHAR(100) NOT NULL,
    [Timestamp] DATETIME NOT NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Reported',
    [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE()
);

-- Optional indexes for better query performance
CREATE NONCLUSTERED INDEX IX_Incidents_Timestamp 
ON [dbo].[Incidents] ([Timestamp] DESC);

CREATE NONCLUSTERED INDEX IX_Incidents_Status 
ON [dbo].[Incidents] ([Status]);