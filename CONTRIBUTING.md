# 🗺 Choose Your Own Contributing Adventure

Did you [**find a bug**](#bug), have an idea for a [**new feature or change**](#idea) or just [**want to help out**](#help-out)?

<br><br><br><br>

## <a id="bug" /> 🐛 I found a bug!

Great! You can either open a [**bug report**](#bug-report) or submit a [**fix**](#fix-bug) for it.
Both are useful options for maintaining this project - choose whichever you feel comfortable doing.

<br><br>

### <a id="bug-report" /> ✍ I'll report the bug!

Detailed issues are very helpful for tracking down the source of the problem.
We don't want double-ups of issues so make sure you check that there isn't [an existing issue already open](https://github.com/TurnerSoftware/CacheTower/issues)!

If there are no existing issues match the bug you've found, you'll need to write out a new one.
The more useful the information you provide, the faster the problem can be solved.

Ideally you would want to include:

- NuGet package version
- .NET runtime version
- Steps to reproduce the issue
- A _minimal_, reproducible example
- Operating system (Windows/Linux/Mac)

If you think you're ready, [**submit your bug report**](https://github.com/TurnerSoftware/CacheTower/issues/new?labels=bug&template=BUG_REPORT.md)!

<br><br>

### <a id="bug-fix" /> 💻 I'll fix the bug!

That's great to hear! Here are some tips for a helpful bug fix:

- It is a good idea to add a test that triggers this specific bug so we can confirm the fix works (also helpful for preventing regressions later on)
- When developing a fix, make sure you follow the coding styles you see within the repository otherwise you might have to redo the changes!
- If there is an issue open for the bug, make sure to tag that issue in your PR description

You probably want to run the tests locally too.
Cache Tower has [some requirements for local testing](#requirements-for-local-testing) which may affect your ability to run the full test suite.

Got that bug fixed? [**Submit your pull request**](https://github.com/TurnerSoftware/CacheTower/compare)!

<br><br><br><br><br><br>

## <a id="idea" /> 💡 I've got an idea for a new feature or change!

Features are great! Is yours a [**small feature**](#idea-small) or a [**big feature**](#idea-big)?
Both are welcome though smaller features are likely to be handled quicker than bigger features.

<br><br>

### <a id="idea-small" /> 🤏 It's a small feature

Small features usually don't take much time on the maintainer's side and not a lot of time on your side.
Do you want to [**suggest the feature**](#idea-small-suggestion) or try a hand at [**implementing the feature**](#idea-small-implementation)?

<br>

#### <a id="idea-small-suggestion" /> ✍ I'll suggest the small feature

Nothing wrong with suggesting a feature!
Keep in mind though that there is only so many hours in the day - even small features may take a while before they are reviewed.

Sometimes features just aren't meant to be and won't get implemented.
It isn't a personal statement if your feature isn't implemented - it might simply not fit in with "the vision" of the project or even conflict with planned changes.

Here are some tips for a good small feature suggestion:

- Describe what problem the feature is solving
- Show an example of how you might use/interact with the feature

If you still want to go ahead, [**submit your feature request**](https://github.com/TurnerSoftware/CacheTower/issues/new?labels=enhancement&template=FEATURE_REQUEST.md)!

<br>

#### <a id="idea-small-implementation" /> 💻 I'll implement the small feature

Nice! Here are some tips for a useful feature implementation:

- Features are only as good as the documentation around them. Make sure the documentation is updated appropriately.
- Please add tests! It doesn't need to be perfect code coverage but the bulk behaviour of the change should be tested.
- Keep to the coding styles you see within the repository otherwise you might have to redo the changes!
- If there is an issue open for the feature, make sure to tag that issue in your PR description

You probably want to run the tests locally too.
Cache Tower has [some requirements for local testing](#requirements-for-local-testing) which may affect your ability to run the full test suite.

Finished your implementation? [**Submit your pull request**](https://github.com/TurnerSoftware/CacheTower/compare)!

<br><br>

### <a id="idea-big" /> 🙌 It's a big feature!

Big features can either be awesome for a project or a burden to it.
It is **highly** recommended to raise an issue about a big feature rather than open a pull request for it.

Big features have to be carefully considered - whether they fit in with "the vision" of the project or potentially conflict with planned changes.
Architectural decisions about the implementation may also need to be discussed to avoid breaking changes or performance penalities.

With these things in mind, a big feature could be pending for months or may _never_ be implemented.

Here are some tips for a good big feature suggestion:

- Describe what problem the feature is solving
- Show an example of how you might use/interact with the feature

If you still want to go ahead, [**submit your feature request**](https://github.com/TurnerSoftware/CacheTower/issues/new?labels=enhancement&template=FEATURE_REQUEST.md)!

<br><br><br><br><br><br>

## <a id="help-out" /> 🙋‍ I just want to help out!

Helpers are always welcome! Feel free to [**triage any open issues**](https://github.com/TurnerSoftware/CacheTower/issues) or make sure the documentation is up-to-date!

If you want to do some coding, you could [**implement any open small suggested features**](#idea-small-implementation) which can bring the idea to reality.




<br><br><br>
<br><br><br>
<br><br><br>

## Miscellaneous

### Requirements for Local Testing

Cache Tower uses external services to perform integration testing.
To run all of the tests, you will need both Redis (or [compatible software](https://www.memurai.com/)) and MongoDB installed.

For Redis, it needs to be at least version 5 compatible.
The tests use the default connection of `localhost:6379` but can be overriden by environment variable `REDIS_ENDPOINT`.
 
For MongoDB, it needs to be at least version 3.
The tests use the default connection string of `mongodb://localhost` but can be overridden by environment variable `MONGODB_URI`.

Back to [**fixing a bug**](#bug-fix) or [**implementing a small feature**](#idea-small-implementation).

<br><br><br>
<br><br><br>