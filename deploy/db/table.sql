USE [OrderDB]
GO

/****** Object:  Table [dbo].[order_processing_info]    Script Date: 03/07/2016 22:17:31 ******/
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'order_processing_info')
BEGIN

CREATE TABLE [dbo].[order_processing_info](
	[id] [varchar](128) NOT NULL,
	[detail] [varchar](max) NOT NULL,
	[status] [varchar](30) NOT NULL,
	[start_time] [datetime] NULL,
	[complete_time] [datetime] NULL,
	[processing_node_id] [varchar](50) NOT NULL,
	[last_update_time] [datetime] NOT NULL,
	[create_time] [datetime] NOT NULL,
	[steps_info] [varchar](max) NOT NULL,
	[timestamp] [timestamp] NOT NULL,
	[tracking_id] [varchar](max) NOT NULL,
 CONSTRAINT [PK_OrderProcessingInfo1] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[order_processing_info] ADD  CONSTRAINT [DF_OrderProcessingInfo_detail]  DEFAULT ('') FOR [detail]
GO

ALTER TABLE [dbo].[order_processing_info] ADD  CONSTRAINT [DF_OrderProcessingInfo_status]  DEFAULT ('') FOR [status]
GO

ALTER TABLE [dbo].[order_processing_info] ADD  CONSTRAINT [DF_OrderProcessingInfo_processingnodeid]  DEFAULT ('') FOR [processing_node_id]
GO

ALTER TABLE [dbo].[order_processing_info] ADD  CONSTRAINT [DF_OrderProcessingInfo_stepsinfo]  DEFAULT ('') FOR [steps_info]
GO

ALTER TABLE [dbo].[order_processing_info] ADD  CONSTRAINT [DF_OrderProcessingInfo_trackingid]  DEFAULT ('') FOR [tracking_id]
GO
END


/****** Object:  Table [dbo].[work_nodes]    Script Date: 03/07/2016 22:18:14 ******/
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'work_nodes')
BEGIN

CREATE TABLE [dbo].[work_nodes](
	[work_node_id] [varchar](50) NOT NULL,
	[start_time] [datetime] NOT NULL,
	[heart_beat_time] [datetime] NOT NULL,
	[processing_order_count] [int] NOT NULL,
 CONSTRAINT [PK_work_nodes] PRIMARY KEY CLUSTERED 
(
	[work_node_id] ASC
)
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[work_nodes] ADD  CONSTRAINT [DF_work_nodes_processing_order_count]  DEFAULT ((0)) FOR [processing_order_count]
GO

END



