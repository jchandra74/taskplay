# Overview
This is a code sample that I created while trying to learn how to do async/await TPL programming where in it we need to do multiple batch processing (i.e. of files) where we might need to do multiple things to a particular file that we want to process.

The idea is to create an asynchronous processing pipeline where each component in the pipeline takes a context object, do something with it and then spit it back out for the next component down the line in the continuation chain.

Some of the pipeline components might perform asynchronous operation internally and some might only perform synchronous operation, or both.  Some might have its own chain internally.

I want to figure out a way to make all of these work in the application.

## Not In Scope (Maybe Later)

Things that has not been touch yet is exception handling.  Need to figure out how to do exception handling in each pipeline instance so when one pipeline breaks, the rest of them will still be running to completion.

# About the Application
The application is basically a .NET Core Console application that will process all the files it finds in the `input` folder in an asynchronous manner.

Each of the files will go through a bunch of things:
1. the program will read its content
2. convert the content into an integer value
3. show you the value it found in the file
4. multiple the original value by 5
5. show you the multipliation result
5. simulate random long work using `Task.Delay`
6. finally write the result into another file in the `output` folder.

The main program will wait for all the files to be processed and outputed and terminate itself after.

The processing of each file is random and non-deterministic.  Meaning do not expect file 1 to be processed first, file 2 next, etc.  You will see this in the `Console` output.

The point of the exercise is to ensure that all files are executed prior to the program termination.

Along the way, I want to show when is the appropriate time to use `.ConfigureAwait(false)` and when it is not appropriate.  I also want to demonstrate hwo to chain Task using `.ContinueWith` and the necessary usage of `.Unwrap` method to reduce the confusion of having to handle `Task<Task<T>>` when doing chaining.

All the codes are in `Program.cs` and is heavily commented, so it should be easy to follow.

The code is meant to be simplistic, so I am including any logging, configuration, dependency injection, etc. intentionally.

Of course, if you are doing a proper application, it is highly recommended to do so, but this one is **intentionally** dumbed down to the important point (dealing with TPL, async/await, Task continuation and chaining) to remove the other complexities.


# How to Run
Assuming that you already have .NET Core SDK / Runtime installed and also git.

_I am using .NET Core 2.2.  Can't guarantee that it will run in older versions_.

git clone this repository and run it from your local directory where you cloned it using `dotnet run`.

# Asking for Feedback
I am hopeful that I am doing all the right things here, but I think I might have missed a few points here and there.

If nothing else, I want to share my learning and see if we can improve it so it can become a good example on how to do this kind of thing.

As I am not an expert in .NET async/await and TPL stuff, any feedback is welcome.
