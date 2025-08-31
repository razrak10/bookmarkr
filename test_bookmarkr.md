I need to see the actual implementation files to write meaningful tests. From the Program.cs file, I can see there are command handlers and services registered, but I need to examine their actual implementations to write proper tests.

**Plan:**
1. Analyze each class implementation to understand the business logic
2. Identify dependencies and their interfaces
3. Write comprehensive tests covering different scenarios and edge cases
4. Use Moq for mocking dependencies
5. Follow Arrange-Act-Assert pattern with NUnit constraint model
6. Validate that the logic makes sense and test both happy path and error scenarios

Could you please share the implementation files for the classes you want tested? Specifically, I need to see:

**Command Handlers:**
- `RootCommandHandler`
- `InteractiveCommandHandler` 
- `LinkCommandHandler`
- `LinkAddCommandHandler`
- `LinkRemoveCommandHandler` 
- `LinkUpdateCommandHandler`
- `ExportCommandHandler`
- `ImportCommandHandler`
- `ShowCommandHandler`
- `CategoryCommandHandler`
- `ChangeCommandHandler`
- `SyncCommandHandler`

**Services:**
- `BookmarkService` (and `IBookmarkService`)
- `BookmarkRepository` (and `IBookmarkRepository`)

**Service Agents:**
- `BookmarkrSyncrServiceAgent` (and `IBookmarkrSyncrServiceAgent`)
- `BookmarkrLookupServiceAgent` (and `IBookmarkrLookupServiceAgent`)

**Other classes:**
- `BookmarkrDbContext`
- Any command classes (like `InteractiveCommand`, `LinkCommand`, etc.)

Without seeing the actual implementations, I cannot write meaningful tests that verify the correct behavior and logic. Please share these files so I can analyze their implementations and write comprehensive unit tests.