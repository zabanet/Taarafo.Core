﻿// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE TO CONNECT THE WORLD
// ---------------------------------------------------------------

using System;
using Taarafo.Core.Models.Posts;
using Taarafo.Core.Models.Posts.Exceptions;

namespace Taarafo.Core.Services.Foundations.Posts
{
    public partial class PostService
    {
        private void ValidatePostOnAdd(Post post)
        {
            ValidatePostIsNotNull(post);

            Validate(
                (Rule: IsInvalid(post.Id), Parameter: nameof(Post.Id)),
                (Rule: IsInvalid(post.Content), Parameter: nameof(Post.Content)),
                (Rule: IsInvalid(post.Author), Parameter: nameof(Post.Author)),
                (Rule: IsInvalid(post.CreatedDate), Parameter: nameof(Post.CreatedDate)),
                (Rule: IsInvalid(post.UpdatedDate), Parameter: nameof(Post.UpdatedDate)),

                (Rule: IsNotSame(
                    firstDate: post.UpdatedDate,
                    secondDate: post.CreatedDate,
                    secondDateName: nameof(Post.CreatedDate)),
                Parameter: nameof(Post.UpdatedDate)),

                (Rule: IsNotRecent(post.CreatedDate), Parameter: nameof(Post.CreatedDate)));
        }

        private void ValidatePostOnModify(Post post)
        {
            ValidatePostIsNotNull(post);

            Validate
            (
                (Rule: IsInvalid(post.Id), Parameter: nameof(post.Id)),
                (Rule: IsInvalid(post.Content), Parameter: nameof(post.Content)),
                (Rule: IsInvalid(post.Author), Parameter: nameof(post.Author)),
                (Rule: IsInvalid(post.CreatedDate), Parameter: nameof(post.CreatedDate)),
                (Rule: IsInvalid(post.UpdatedDate), Parameter: nameof(post.UpdatedDate)),
                (Rule: IsNotRecent(post.UpdatedDate), Parameter: nameof(post.UpdatedDate)),

                (Rule: IsSame(
                    firstDate: post.UpdatedDate,
                    secondDate: post.CreatedDate,
                    secondDateName: nameof(Post.CreatedDate)),

                Parameter: nameof(Post.UpdatedDate))
            );
        }

        public void ValidatePostId(Guid postId) =>
           Validate((Rule: IsInvalid(postId), Parameter: nameof(Post.Id)));

        private static void ValidatePostIsNotNull(Post post)
        {
            if (post is null)
            {
                throw new NullPostException();
            }
        }

        private static void ValidateStoragePost(Post maybePost, Guid postId)
        {
            if (maybePost is null)
            {
                throw new NotFoundPostException(postId);
            }
        }

        private static dynamic IsInvalid(Guid id) => new
        {
            Condition = id == Guid.Empty,
            Message = "Id is required"
        };

        private static dynamic IsInvalid(DateTimeOffset date) => new
        {
            Condition = date == default,
            Message = "Date is required"
        };

        private static dynamic IsNotSame(
            DateTimeOffset firstDate,
            DateTimeOffset secondDate,
            string secondDateName) => new
            {
                Condition = firstDate != secondDate,
                Message = $"Date is not the same as {secondDateName}"
            };

        private static dynamic IsSame(
            DateTimeOffset firstDate,
            DateTimeOffset secondDate,
            string secondDateName) => new
            {
                Condition = firstDate == secondDate,
                Message = $"Date is the same as {secondDateName}"
            };

        private static dynamic IsInvalid(string text) => new
        {
            Condition = String.IsNullOrWhiteSpace(text),
            Message = "Text is required"
        };

        private dynamic IsNotRecent(DateTimeOffset date) => new
        {
            Condition = IsDateNotRecent(date),
            Message = "Date is not recent"
        };

        private bool IsDateNotRecent(DateTimeOffset date)
        {
            DateTimeOffset currentDateTime =
                this.dateTimeBroker.GetCurrentDateTimeOffset();

            TimeSpan timeDifference = currentDateTime.Subtract(date);
            TimeSpan oneMinute = TimeSpan.FromMinutes(1);

            return timeDifference.Duration() > oneMinute;
        }
        private static void ValidateAginstStoragePostOnModify(Post inputPost, Post storagePost)
        {
            Validate(
                (Rule: IsNotSame(
                    firstDate: inputPost.CreatedDate,
                    secondDate: storagePost.CreatedDate,
                    secondDateName: nameof(Post.CreatedDate)),
                Parameter: nameof(Post.CreatedDate)),

                (Rule: IsSame(
                    firstDate: inputPost.UpdatedDate,
                    secondDate: storagePost.UpdatedDate,
                    secondDateName: nameof(Post.UpdatedDate)),
                Parameter: nameof(Post.UpdatedDate)));
        }

        private static void Validate(params (dynamic Rule, string Parameter)[] validations)
        {
            var invalidPostException = new InvalidPostException();

            foreach ((dynamic rule, string parameter) in validations)
            {
                if (rule.Condition)
                {
                    invalidPostException.UpsertDataList(
                        key: parameter,
                        value: rule.Message);
                }
            }

            invalidPostException.ThrowIfContainsErrors();
        }
    }
}
