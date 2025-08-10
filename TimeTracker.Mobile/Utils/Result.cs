// TimeTracker.Mobile/Utils/Result.cs
#nullable enable
using System;
using System.Threading.Tasks;

namespace TimeTracker.Mobile.Utils
{
    /// <summary>
    /// Monade Result générique pour véhiculer un succès (Value) ou un échec (Error).
    /// - Rétro-compatible avec l’API existante: Success(value), Fail(error).
    /// - Champs additionnels: StatusCode (HTTP éventuel), Exception (jamais affichée en UI par défaut).
    /// - Utilitaires: Try/TryAsync, Map, Bind, Ensure, Match, Deconstruct.
    /// </summary>
    public sealed class Result<T>
    {
        public bool IsSuccess { get; }
        public T? Value { get; }
        public string? Error { get; }
        public int? StatusCode { get; }
        public Exception? Exception { get; }

        private Result(bool isSuccess, T? value, string? error, int? statusCode, Exception? exception)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
            StatusCode = statusCode;
            Exception = exception;
        }

        // --- Fabriques rétro-compatibles ---
        public static Result<T> Success(T value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return new(true, value, null, null, null);
        }

        public static Result<T> Fail(string error) => Fail(error, null, null);

        // --- Surplus utiles ---
        public static Result<T> Fail(string error, int? statusCode, Exception? exception)
        {
            ArgumentException.ThrowIfNullOrEmpty(error);
            return new(false, default, error, statusCode, exception);
        }

        /// <summary>Exécute une fonction et capture l’exception en échec.</summary>
        public static Result<T> Try(Func<T> func, string? error = null)
        {
            try
            {
                var v = func();
                return Success(v);
            }
            catch (Exception ex)
            {
                return Fail(error ?? "Operation failed.", null, ex);
            }
        }

        /// <summary>Exécute une fonction async et capture l’exception en échec.</summary>
        public static async Task<Result<T>> TryAsync(Func<Task<T>> func, string? error = null)
        {
            try
            {
                var v = await func().ConfigureAwait(false);
                return Success(v);
            }
            catch (Exception ex)
            {
                return Fail(error ?? "Operation failed.", null, ex);
            }
        }

        /// <summary>Transforme la valeur en cas de succès, sinon propage l’erreur.</summary>
        public Result<U> Map<U>(Func<T, U> mapper)
        {
            if (!IsSuccess || Value is null)
                return Result<U>.Fail(Error ?? "Operation failed.", StatusCode, Exception);

            return Result<U>.Try(() => mapper(Value), Error);
        }

        /// <summary>Chaîne une opération qui renvoie elle-même un Result.</summary>
        public Result<U> Bind<U>(Func<T, Result<U>> binder)
        {
            if (!IsSuccess || Value is null)
                return Result<U>.Fail(Error ?? "Operation failed.", StatusCode, Exception);

            try
            {
                return binder(Value);
            }
            catch (Exception ex)
            {
                return Result<U>.Fail("Operation failed.", StatusCode, ex);
            }
        }

        /// <summary>Valide une condition sur la valeur; échoue sinon.</summary>
        public Result<T> Ensure(Func<T, bool> predicate, string errorIfFalse)
        {
            if (IsSuccess && Value is not null && predicate(Value))
                return this;

            return Fail(string.IsNullOrWhiteSpace(errorIfFalse) ? "Validation failed." : errorIfFalse,
                        StatusCode, Exception);
        }

        /// <summary>Déstructure en (isSuccess, value, error).</summary>
        public void Deconstruct(out bool isSuccess, out T? value, out string? error)
        {
            isSuccess = IsSuccess;
            value = Value;
            error = Error;
        }

        /// <summary>Switch expression friendly.</summary>
        public U Match<U>(Func<T, U> onSuccess, Func<string?, U> onFailure) =>
            (IsSuccess && Value is not null) ? onSuccess(Value) : onFailure(Error);

        public override string ToString() =>
            IsSuccess ? $"Success({Value})" : $"Fail({Error}{(StatusCode is { } c ? $", {c}" : "")})";

        // Implicite pratique (optionnel, non requis par ton code actuel)
        public static implicit operator Result<T>(T value) => Success(value);
    }
}
