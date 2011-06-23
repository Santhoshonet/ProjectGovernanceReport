-- ITXPGReportDataLayer.Groups
CREATE TABLE [groups] (
    [groups_id] INT NOT NULL,               -- <internal-pk>
    [<_u_i_d>k___backing_field] VARCHAR(255) NULL, -- <UID>k__BackingField
    [<name>k___backing_field] VARCHAR(255) NULL, -- <name>k__BackingField
    [voa_version] SMALLINT NOT NULL,        -- <internal-version>
    CONSTRAINT [pk_groups] PRIMARY KEY ([groups_id])
)
go

-- System.Collections.Generic.IList`1[[ITXPGReportDataLayer.Projects, ITXPGReportDataLayer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]] ITXPGReportDataLayer.Groups.<projects>k__BackingField
CREATE TABLE [groups_projects] (
    [groups_id] INT NOT NULL,
    [seq] INT NOT NULL,                     -- <sequence>
    [projects_id] INT NULL,
    CONSTRAINT [pk_groups_projects] PRIMARY KEY ([groups_id], [seq])
)
go

-- ITXPGReportDataLayer.Projects
CREATE TABLE [projects] (
    [projects_id] INT NOT NULL,             -- <internal-pk>
    [<name>k___backing_field] VARCHAR(255) NULL, -- <name>k__BackingField
    [<uid>k___backing_field] VARCHAR(255) NULL, -- <uid>k__BackingField
    [voa_version] SMALLINT NOT NULL,        -- <internal-version>
    CONSTRAINT [pk_projects] PRIMARY KEY ([projects_id])
)
go

-- ITXPGReportDataLayer.Users
CREATE TABLE [users] (
    [users_id] INT NOT NULL,                -- <internal-pk>
    [<_rsrc_u_i_d>k___backing_field] VARCHAR(255) NULL, -- <ResourceUID>k__BackingField
    [voa_version] SMALLINT NOT NULL,        -- <internal-version>
    CONSTRAINT [pk_users] PRIMARY KEY ([users_id])
)
go

-- System.Collections.Generic.IList`1[[ITXPGReportDataLayer.Groups, ITXPGReportDataLayer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]] ITXPGReportDataLayer.Users.<groups>k__BackingField
CREATE TABLE [users_groups] (
    [users_id] INT NOT NULL,
    [seq] INT NOT NULL,                     -- <sequence>
    [groups_id] INT NULL,
    CONSTRAINT [pk_users_groups] PRIMARY KEY ([users_id], [seq])
)
go

-- OpenAccessRuntime.Relational.sql.HighLowRelationalKeyGenerator
CREATE TABLE [voa_keygen] (
    [table_name] VARCHAR(64) NOT NULL,
    [last_used_id] INT NOT NULL,
    CONSTRAINT [pk_voa_keygen] PRIMARY KEY ([table_name])
)
go

ALTER TABLE [groups_projects] ADD CONSTRAINT [ref_groups_projects_groups] FOREIGN KEY ([groups_id]) REFERENCES [groups]([groups_id])
go

ALTER TABLE [groups_projects] ADD CONSTRAINT [ref_groups_projects_projects] FOREIGN KEY ([projects_id]) REFERENCES [projects]([projects_id])
go

ALTER TABLE [users_groups] ADD CONSTRAINT [ref_users_groups_users] FOREIGN KEY ([users_id]) REFERENCES [users]([users_id])
go

ALTER TABLE [users_groups] ADD CONSTRAINT [ref_users_groups_groups] FOREIGN KEY ([groups_id]) REFERENCES [groups]([groups_id])
go

