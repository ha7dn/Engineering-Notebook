// Example of a rich domain model with encapsulated state and behavior. Source: https://milanjovanovic.tech/blog/what-invariants-are-and-why-a-domain-model-is-the-best-place-to-enforce-them

public class Course
{  
    private readonly List _lessons = [];
    public IReadOnlyCollection Lessons => _lessons.AsReadOnly();

    private Course(CourseId id, string title, Money price)
    { Id = id; Title = title; Price = price; Status = CourseStatus.Draft; }

    // Block construction of invalid objects by using a Static factory method to create a Course instance.
    public static Result Create(string title, Money price)
    {
        if (price.Amount < 0)
        {
            // A Course with a negative price is not valid, so we return an error instead of a Course instance.
            return CourseErrors.PriceCannotBeNegative;
        }
        if (string.IsNullOrWhiteSpace(title))
        {
            // A Course without a title is not valid, so we return an error instead of a Course instance.
            return CourseErrors.TitleRequired;
        }
        return new Course(CourseId.New(), title, price);
    }

    // Encapsulate state changes.
    public Result Publish(IDateTimeProvider clock)
    {
        if (Status != CourseStatus.Draft)
        {
            return CourseErrors.AlreadyPublished;
        }
        if (_lessons.Count == 0)
        {
            return CourseErrors.CannotPublishWithoutLessons;
        }
        Status = CourseStatus.Published; PublishedOn = clock.UtcNow; return Result.Success();
    }

    // Encapsulate the aggregate invariants.
    public Result RemoveLesson(LessonId id)
    {
        if (Status == CourseStatus.Published)
        {
            return CourseErrors.CannotModifyPublishedLessons;
        }
        var lesson = _lessons.FirstOrDefault(l => l.Id == id);
        if (lesson is null)
        {
            return CourseErrors.LessonNotFound;
        }
        _lessons.Remove(lesson);
        return Result.Success();
    }
}