# Privileged

Privileged is an isomorphic authorization library for restricting resources by action, subjct and fields. 
It's designed to be incrementally adoptable and can easily scale between a simple claim based and fully featured 
subject and action based authorization. It makes it easy to manage and share permissions across UI components, 
API services, and database queries.

Inspired by [CASL](https://github.com/stalniy/casl)

[![Build Project](https://github.com/loresoft/Privileged/actions/workflows/dotnet.yml/badge.svg)](https://github.com/loresoft/Privileged/actions/workflows/dotnet.yml)

[![Coverage Status](https://coveralls.io/repos/github/loresoft/Privileged/badge.svg?branch=main)](https://coveralls.io/github/loresoft/Privileged?branch=main)

[![Privileged](https://img.shields.io/nuget/v/Privileged.svg)](https://www.nuget.org/packages/Privileged/)
                                                                                                                                
## Features

* **Versatile** An incrementally adoptable and can easily scale between a simple claim based and fully featured subject and attribute based authorization.
* **Isomorphic** Can be used on frontend and backend and complementary packages make integration with Frontend and Backend effortless
* **Declarative** Thanks to declarative rules, you can serialize and share permissions between UI and API or microservices

## General

Privileged operates on rules for what a user can actually do in the application. A rule itself depends on the 3 parameters:

1. **Action**  Describes what user can actually do in the app. User action is a word (usually a verb) which depends on the business logic (e.g., `update`, `read`). Very often it will be a list of words from CRUD - `create`, `read`, `update` and `delete`.
2. **Subject**  The subject which you want to check user action on. Usually this is a business (or domain) entity name (e.g., `Subscription`, `Post`, `User`).
3. **Fields**  Can be used to restrict user action only to matched subject's fields (e.g., to allow moderator to update `published` field of `Post` but not update `description` or `title`)

## Examples

Using builder to create rules

```c#
var context = new PrivilegeBuilder()
    .Allow("test", PrivilegeSubjects.All)
    .Allow(PrivilegeActions.All, "Post")
    .Forbid("publish", "Post")
    .Build();

context.Allowed("read", "Post").Should().BeTrue();
context.Allowed("update", "Post").Should().BeTrue();
context.Allowed("archive", "Post").Should().BeTrue();
context.Allowed("read", "User").Should().BeFalse();
context.Allowed("delete", "Post").Should().BeTrue();
context.Allowed("publish", "Post").Should().BeFalse();
context.Allowed("test", "User").Should().BeTrue();
context.Allowed("test", "Post").Should().BeTrue();
```

Using fields

```c#
var context = new PrivilegeBuilder()
    .Allow("read", "Post", ["title", "id"])
    .Allow("read", "User")
    .Build();

context.Allowed("read", "Post").Should().BeTrue();
context.Allowed("read", "Post", "id").Should().BeTrue();
context.Allowed("read", "Post", "title").Should().BeTrue();
context.Allowed("read", "Post", "ssn").Should().BeFalse();

context.Allowed("read", "User").Should().BeTrue();
context.Allowed("read", "User", "id").Should().BeTrue();
```
