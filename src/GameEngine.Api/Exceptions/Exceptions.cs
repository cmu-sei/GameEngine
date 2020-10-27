// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using System;

namespace GameEngine.Exceptions
{
    public class ProblemPendingException : Exception
    {
        public ProblemPendingException()
        : base("This request is already being processed.") { }

    }

    public class ResourcesFullException : Exception
    {
        public ResourcesFullException()
        : base("All seats are currently filled.") { }

    }

    public class EngineBusyException : Exception
    {
        public EngineBusyException()
        : base("GameEngine is busy. Try again in a minute.") { }

    }

    public class ChallengeNotFoundException : Exception
    {
        public ChallengeNotFoundException()
        : base("Challenge not found.") { }
    }

    public class TokenSpecNotFoundException : Exception
    {
        public TokenSpecNotFoundException()
        : base("Flag not found.") { }
    }

    public class GradeCommandEmptyException : Exception
    {
        public GradeCommandEmptyException()
            : base("Grade Command does not exist for this flag.") { }
    }

    public class ProblemGenerationException : Exception
    {
        public ProblemGenerationException()
        : base("Failed to generate problem.") { }
    }

    public class ProblemGenerationTimeoutException : Exception
    {
        public ProblemGenerationTimeoutException()
        : base("Problem generation timed out.") { }
    }

    public class ProblemGradingTimeoutException : Exception
    {
        public ProblemGradingTimeoutException()
        : base("Problem grading timed out.") { }
    }

    public class ProblemGradingException : Exception
    {
        public ProblemGradingException()
        : base("Problem encountered during grading.") { }

        public ProblemGradingException(string message)
        : base(message) { }
    }

    public class NotFoundException : Exception
    {
        public NotFoundException()
        : base("Item not found.") { }
    }

    public class ProblemCompleteException : Exception
    {
        public ProblemCompleteException()
        : base("A completed problem cannot be changed.") { }
    }
}

