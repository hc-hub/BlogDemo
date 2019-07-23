using FluentValidation;

namespace BlogDemo.Infrastructure.Resources
{
    public class PostAddOrUpdateResourceValidator<T>:AbstractValidator<T> where T:PostAddOrUpdateResource
    {
        public PostAddOrUpdateResourceValidator()
        {
            RuleFor(x => x.Title)
                .NotNull()
                .WithName("标题")
                .WithMessage("requied|{PropertyName}是必填的")
                .MaximumLength(50)
                .WithMessage("requied|{PropertyName}的最大长度是{MaxLength}");
            RuleFor(x => x.Body)
                .NotNull()
                .WithName("正文")
                .WithMessage("requied|{PropertyName}是必填的")
                .MinimumLength(100)
                .WithMessage("requied|{PropertyName}的最小长度是{MinLength}");
        }
    }
}
