create table Roles (Id int4 not null, Name varchar(255), ApplicationName varchar(255), primary key (Id),unique (Name, ApplicationName))
create table Roles_Users (RoleFk int4 not null, UserFk int4 not null, primary key (UserFk, RoleFk))
create table Profiles (Id int4 not null, ApplicationName varchar(255), IsAnonymous boolean, LastActivityDate timestamp, LastUpdatedDate timestamp, UserFk int4, primary key (Id))
create table Sessions (Id int4 not null, Data text, SessionId varchar(255), ApplicationName varchar(255), Created timestamp, Expires timestamp, Timeout int4, Locked boolean, LockId int4, LockDate timestamp, Flags int4, primary key (Id),unique (SessionId))
create table Users (Id int4 not null, Username varchar(255), ApplicationName varchar(255), Email varchar(255), IsLockedOut boolean, Comment varchar(255), Password varchar(255), PasswordQuestion varchar(255), PasswordAnswer varchar(255), IsApproved boolean, LastActivityDate timestamp, LastLoginDate timestamp, LastPasswordChangedDate timestamp, CreationDate timestamp, IsOnline boolean, LastLockedOutDate timestamp, FailedPasswordAttemptCount int4, FailedPasswordAttemptWindowStart timestamp, FailedPasswordAnswerAttemptCount int4, FailedPasswordAnswerAttemptWindowStart timestamp, UserProfileFk int4, primary key (Id),unique (Username, ApplicationName))
create table OpenIdAlternatives (User_id int4 not null, Value varchar(255))
create table ProfileDatas (Id int4 not null, ValueString text, Name varchar(255), ValueBinary bytea, ProfileFk int4, primary key (Id))
alter table Roles_Users add constraint FKCB323D5A638CFADF foreign key (UserFk) references Users
alter table Roles_Users add constraint FKCB323D5A9757D337 foreign key (RoleFk) references Roles
alter table Profiles add constraint FK85C1A0B638CFADF foreign key (UserFk) references Users
create index users_email_index on Users (Email)
create index users_islockedout_index on Users (IsLockedOut)
alter table Users add constraint FK2C1C7FE59DABAF15 foreign key (UserProfileFk) references Profiles
alter table OpenIdAlternatives add constraint FKDFDB839038B0E3C4 foreign key (User_id) references Users
alter table ProfileDatas add constraint FKBA918E802DDA97A3 foreign key (ProfileFk) references Profiles
create table hibernate_unique_key ( next_hi int4 )
insert into hibernate_unique_key values ( 1 )
