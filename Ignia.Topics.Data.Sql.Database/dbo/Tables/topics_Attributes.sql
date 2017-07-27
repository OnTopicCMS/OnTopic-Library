CREATE TABLE [dbo].[topics_Attributes] (
    [AttributeID]     INT            IDENTITY (1, 1) NOT NULL,
    [AttributeKey]    NVARCHAR (124) NOT NULL,
    [DisplayName]     NVARCHAR (124) NULL,
    [DisplayGroup]    NVARCHAR (124) NULL,
    [AttributeTypeID] INT            NOT NULL,
    [Description]     NVARCHAR (255) NULL,
    [SortOrder]       TINYINT        CONSTRAINT [DF_topics_Attributes_SortOrder] DEFAULT ((25)) NOT NULL,
    [IsVisible]       BIT            CONSTRAINT [DF_topics_Attributes_IsVisible] DEFAULT ((1)) NOT NULL,
    [IsRequired]      BIT            CONSTRAINT [DF_topics_Attributes_IsRequired] DEFAULT ((0)) NOT NULL,
    [DefaultValue]    NVARCHAR (124) NULL,
    [StoreInBlob]     BIT            CONSTRAINT [DF_topics_Attributes_IsBlob] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_topics_Attributes] PRIMARY KEY CLUSTERED ([AttributeID] ASC)
);

