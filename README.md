# InstaBot
Instagram automation bot in C# using selenium.

# Building
Build with dotnet:

dotnet publish -c Release -r <system>

# Folder Creation
Requires the following to be created:

Folder users/ with file named the account name (no extension) see below
Folder inactive/ keep inactive account files here
Folder stats/ to store account stats

# Creation of Account File
Create a file with the same name as the account username (no extension)

Copy following into file:

email=
password=
tags=
comments=
skipRate=
followRate=
commentRate=
unfollow=

Set email(string) to account email
Set password(string) to account password
Set tags(string) to tags to be explore with comma seperation i.e: extag1,extag2,extag3
Set comments(string) to comments to be used with comma seperation i.e: excomment1,excomment2,excomment3
Set skipRate(int 0-100) to the percentage of posts to be skipped e.i: 10 (10 Recommended)
Set followRate(int 0-100) to the percentage of accounts to be followed e.i: 30 (30 Recommended)
Set commentRate(int 0-100) to the percentage of posts to be commented on e.i: 5 (5 Recommended)
Set unfollow(Boolean) to true if you want the bot to automatically unfollow e.i: true (true Recommended)

Place saved file in users/ and start program to run account or place in inactive/ to leave inactive when program starts

# Reading Statistics
