USE [FtDb]
GO

CREATE TABLE [dbo].[Users](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](50) NULL,
	[Password] [varchar](50) NULL,
	[Email] [varchar](50) NULL,
	[IsMailActivated] [int],
	[Role] [int] NOT NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id] ASC)
	)

GO

INSERT INTO [Users] VALUES ('developer', 'developer', 'developer@tut.by', 0, 1);
INSERT INTO [Users] VALUES ('root', 'root', 'root@tut.by', 1, 2);
INSERT INTO [Users] VALUES ('operator', 'operator', 'operator@tut.by', 0, 3);
INSERT INTO [Users] VALUES ('supervisor', 'supervisor', 'supervisor@tut.by', 1, 4);
INSERT INTO [Users] VALUES ('superclient', 'superclient', 'superclient@tut.by', 0, 5);
