CREATE TABLE [dbo].[topics_Relationships] (
    [Target_TopicID]     INT          NOT NULL,
    [Source_TopicID]     INT          NOT NULL,
    [RelationshipTypeID] VARCHAR (64) NOT NULL,
    CONSTRAINT [PK_Topics_Relationships_1] PRIMARY KEY CLUSTERED ([Target_TopicID] ASC, [Source_TopicID] ASC, [RelationshipTypeID] ASC),
    CONSTRAINT [FK_Topics_Relationships_topics_Topic_Source] FOREIGN KEY ([Source_TopicID]) REFERENCES [dbo].[topics_Topics] ([TopicID]),
    CONSTRAINT [FK_Topics_Relationships_topics_Topic_Target] FOREIGN KEY ([Target_TopicID]) REFERENCES [dbo].[topics_Topics] ([TopicID])
);

