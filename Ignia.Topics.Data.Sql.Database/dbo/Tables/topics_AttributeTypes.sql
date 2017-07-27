CREATE TABLE [dbo].[topics_AttributeTypes] (
    [AttributeTypeID]      INT            NOT NULL,
    [AttributeTypeName]    NVARCHAR (124) NOT NULL,
    [DefaultConfiguration] VARCHAR (124)  CONSTRAINT [DF_topics_AttributeTypes_DefaultConfiguration] DEFAULT ('') NULL,
    CONSTRAINT [PK_topics_AttributeTypes] PRIMARY KEY CLUSTERED ([AttributeTypeID] ASC)
);

