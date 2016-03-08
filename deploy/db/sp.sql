USE [OrderDB]
GO


/****** Object:  StoredProcedure [dbo].[sp_get_processing_info_by_id]    Script Date: 03/07/2016 22:21:34 ******/
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'sp_get_processing_info_by_id' AND TYPE = 'P') DROP PROC sp_get_processing_info_by_id
GO
CREATE PROC [dbo].[sp_get_processing_info_by_id]
(
    @id varchar(128)
)
AS
BEGIN

    SELECT TOP 1 [id]
      ,[detail]
      ,[status]
      ,[start_time]
      ,[complete_time]
      ,[processing_node_id]
      ,[last_update_time]
      ,[create_time]
      ,[steps_info]
      ,[timestamp]
      ,[tracking_id]
  FROM [order_processing_info]
  where id = @id

END
GO

/****** Object:  StoredProcedure [dbo].[sp_get_processing_info_by_tracking_id]    Script Date: 03/07/2016 22:21:56 ******/
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'sp_get_processing_info_by_tracking_id' AND TYPE = 'P') DROP PROC sp_get_processing_info_by_tracking_id
GO
CREATE PROC [dbo].[sp_get_processing_info_by_tracking_id]
(
    @tracking_id varchar(128)
)
AS
BEGIN

    SELECT TOP 1 [id]
      ,[detail]
      ,[status]
      ,[start_time]
      ,[complete_time]
      ,[processing_node_id]
      ,[last_update_time]
      ,[create_time]
      ,[steps_info]
      ,[timestamp]
      ,[tracking_id]
  FROM [order_processing_info]
  where tracking_id = @tracking_id

END
GO


/****** Object:  StoredProcedure [dbo].[sp_create_processing_info]    Script Date: 03/07/2016 22:19:13 ******/
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'sp_create_processing_info' AND TYPE = 'P') DROP PROC sp_create_processing_info
GO
CREATE PROC [dbo].[sp_create_processing_info]
(
    @id varchar(128),
    @tracking_id varchar(128),
    @detail NVARCHAR(MAX)
)
AS
BEGIN
    DECLARE @dt_now DATETIME = GETUTCDATE()

    IF EXISTS (SELECT 1 FROM [dbo].[order_processing_info] WITH (NOLOCK) WHERE id = @id)
    BEGIN
        RAISERROR('ORDER_ID_AREADY_EXISTS', 16, 1)
        RETURN
    END

	IF EXISTS (SELECT 1 FROM [dbo].[order_processing_info] WITH (NOLOCK) WHERE tracking_id = @tracking_id)
    BEGIN
        EXEC [sp_get_processing_info_by_tracking_id] @tracking_id = @tracking_id
    END

	ELSE BEGIN

    INSERT INTO [dbo].[order_processing_info]
           ([id], 
		   [detail],
		   [status],
		   [last_update_time],
		   [create_time], 
		   [tracking_id])
    VALUES
    (
         @id,
		 @detail,
		 'Pending',
		 @dt_now,
		 @dt_now,
         @tracking_id
    )
    EXEC [sp_get_processing_info_by_id] @id=  @id
	END
END
GO


/****** Object:  StoredProcedure [dbo].[sp_get_dead_nodes_processing_infos]    Script Date: 03/07/2016 22:19:58 ******/
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'sp_get_dead_nodes_processing_infos' AND TYPE = 'P') DROP PROC sp_get_dead_nodes_processing_infos
GO
CREATE PROC [dbo].[sp_get_dead_nodes_processing_infos]
(
    @count int,
	@processing_node_id varchar(50),
	@max_no_heart_beat_seconds int
)
AS
BEGIN
	DECLARE @dt_now DATETIME = GetUTCDate()
	DECLARE @before_time DATETIME = DATEADD(SECOND, @max_no_heart_beat_seconds * -1, @dt_now )

    DECLARE @tbl_ids TABLE
	(
		[id] [varchar](50)
	)

	insert into @tbl_ids
	select TOP (@count) [id] 
	from [work_nodes] 
	inner join [order_processing_info]
	on work_nodes.work_node_id= order_processing_info.processing_node_id
	where work_nodes.heart_beat_time < @before_time
	and order_processing_info.status != 'Completed' and order_processing_info.status != 'Failed'
	order by create_time desc

	IF EXISTS (SELECT 1 FROM @tbl_ids )
	BEGIN
	update [order_processing_info]
	set 
	[processing_node_id] = @processing_node_id,
	[last_update_time] = @dt_now
	where [id] in (select id from @tbl_ids)

	SELECT TOP (@count) [id]
		,[detail]
		,[status]
		,[start_time]
		,[complete_time]
		,[processing_node_id]
		,[last_update_time]
		,[create_time]
		,[steps_info]
		,[timestamp]
		,[tracking_id]
	FROM [order_processing_info]
	where [id] in (select id from @tbl_ids)
	order by create_time 

  END
END

/****** Object:  StoredProcedure [dbo].[sp_get_new_processing_infos]    Script Date: 03/07/2016 22:21:14 ******/
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'sp_get_new_processing_infos' AND TYPE = 'P') DROP PROC sp_get_new_processing_infos
GO
CREATE PROC [dbo].[sp_get_new_processing_infos]
(
    @count int,
	@processing_node_id varchar(50)
)
AS
BEGIN
	DECLARE @dt_now DATETIME = GetUTCDate()

    DECLARE @tbl_ids TABLE
	(
		[id] [varchar](128)
	)
    
	insert into @tbl_ids
	select TOP (@count) [id] 
	from [order_processing_info] 
	where [status] = 'Pending' order by [create_time]

	update [order_processing_info]
	set [status] = 'Scheduling',
	[processing_node_id] = @processing_node_id,
	[last_update_time] = @dt_now,
	[start_time] = @dt_now
	where [id] in (select id from @tbl_ids)

    SELECT TOP (@count) [id]
      ,[detail]
      ,[status]
      ,[start_time]
      ,[complete_time]
      ,[processing_node_id]
      ,[last_update_time]
      ,[create_time]
      ,[steps_info]
      ,[timestamp]
      ,[tracking_id]
  FROM [order_processing_info]
  where [id] in (select id from @tbl_ids)
  order by create_time 

END
GO


/****** Object:  StoredProcedure [dbo].[sp_get_timedout_processing_infos]    Script Date: 03/07/2016 22:22:17 ******/
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'sp_get_timedout_processing_infos' AND TYPE = 'P') DROP PROC sp_get_timedout_processing_infos
GO
CREATE PROC [dbo].[sp_get_timedout_processing_infos]
(
    @count int,
	@processing_node_id varchar(50),
	@timed_out_seconds int
)
AS
BEGIN
	DECLARE @dt_now DATETIME = GetUTCDate()
	DECLARE @before_time DATETIME = DATEADD(SECOND, @timed_out_seconds * -1, @dt_now )

    DECLARE @tbl_ids TABLE
	(
		[id] [varchar](128)
	)

	insert into @tbl_ids
	select TOP (@count) [id] 
	from [order_processing_info] 
	where [status] != 'Pending' 
	and [status] != 'Failed' 
	and [status]!='Completed'
	and [last_update_time] < @before_time
	order by [create_time]

	IF EXISTS (SELECT 1 FROM @tbl_ids )
	BEGIN
	update [order_processing_info]
	set 
	[processing_node_id] = @processing_node_id,
	[last_update_time] = @dt_now
	where [id] in (select id from @tbl_ids)

	SELECT TOP (@count) [id]
		,[detail]
		,[status]
		,[start_time]
		,[complete_time]
		,[processing_node_id]
		,[last_update_time]
		,[create_time]
		,[steps_info]
		,[timestamp]
		,[tracking_id]
	FROM [order_processing_info]
	where [id] in (select id from @tbl_ids)
	order by create_time 
  END
END
GO

/****** Object:  StoredProcedure [dbo].[sp_report_node_heart_beat]    Script Date: 03/07/2016 22:22:38 ******/
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'sp_report_node_heart_beat' AND TYPE = 'P') DROP PROC sp_report_node_heart_beat
GO
CREATE PROC [dbo].[sp_report_node_heart_beat]
(
    @work_node_id varchar(50),
	@processing_order_count int
)
AS
BEGIN
	DECLARE @dt_now DATETIME = GetUTCDate()
    
	if EXISTS (select 1 from work_nodes where work_node_id = @work_node_id)
	BEGIN

	  update work_nodes set heart_beat_time=@dt_now, processing_order_count = @processing_order_count

	  where work_node_id = @work_node_id

	END
	ELSE BEGIN

	insert into work_nodes(work_node_id, start_time, heart_beat_time, processing_order_count)

	values(@work_node_id, @dt_now, @dt_now, @processing_order_count)

	END
END
GO

/****** Object:  StoredProcedure [dbo].[sp_report_node_start]    Script Date: 03/07/2016 22:23:05 ******/
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'sp_report_node_start' AND TYPE = 'P') DROP PROC sp_report_node_start
GO
CREATE PROC [dbo].[sp_report_node_start]
(
    @work_node_id varchar(50)
)
AS
BEGIN
	DECLARE @dt_now DATETIME = GetUTCDate()
    
	if EXISTS (select 1 from work_nodes where work_node_id = @work_node_id)
	BEGIN
	  update work_nodes set start_time = @dt_now, heart_beat_time=@dt_now, processing_order_count = 0
	  where work_node_id = @work_node_id
	END
	ELSE BEGIN
	insert into work_nodes(work_node_id, start_time, heart_beat_time, processing_order_count)
	values(@work_node_id, @dt_now, @dt_now, 0)
	END
END

GO

/****** Object:  StoredProcedure [dbo].[sp_update_processing_info]    Script Date: 03/07/2016 22:23:44 ******/
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'sp_update_processing_info' AND TYPE = 'P') DROP PROC sp_update_processing_info
GO
Create PROC [dbo].[sp_update_processing_info]
(
    @id varchar(128),
    @status varchar(50),
    @complete_time datetime,
    @processing_node_id varchar(50),
    @steps_info varchar(MAX),
    @start_time datetime,
    @timestamp timestamp,
    @detail NVARCHAR(MAX)
)
AS
BEGIN

    DECLARE @dt_now DATETIME = GETUTCDATE()

    update order_processing_info
    set [status] = @status,
    complete_time = @complete_time,
    processing_node_id = @processing_node_id,
    steps_info = @steps_info,
    start_time = @start_time,
    detail  =@detail,
    last_update_time = @dt_now
    where id = @id and [timestamp] = @timestamp
    
    if (@@ROWCOUNT = 0)
    BEGIN
		if exists(select 1 from [order_processing_info] where [id] = @id )
        BEGIN
            RAISERROR ('TIMESTAMP_CONFLICT', 16, 1)
        END
        ELSE
        BEGIN
            RAISERROR ('ORDER_NOT_EXIST', 16, 2)
        END
    END
    ELSE BEGIN
     EXEC [sp_get_processing_info_by_id] @id=  @id
	END
END
GO

















