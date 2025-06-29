
// Approves Authentication for the Client to enable Online Services.

using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public static class AuthenticationWrapper
{
    public static AuthState AuthState { get; private set; } = AuthState.NotAuthenticated;

    public static async Task<AuthState> DoAuth(int maxTries = 5)
    {
        if (AuthState == AuthState.Authenticated)
        {
            return AuthState;
        }

        if (AuthState == AuthState.Authenticating)
        {
            Debug.Log("Waiting to authenticate!");
            await Authenticating();
            return AuthState;
        }

        await SignInAnonymouslyAsync(maxTries);

        return AuthState;
    }

    private static async Task SignInAnonymouslyAsync(int maxRetries)
    {
        AuthState = AuthState.Authenticating;

        int retries = 0;
        while (AuthState == AuthState.Authenticating && retries < maxRetries)
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                if (AuthenticationService.Instance.IsAuthorized && AuthenticationService.Instance.IsSignedIn)
                {
                    AuthState = AuthState.Authenticated;
                    break;
                }
            }
            catch(AuthenticationException ex)
            {
                Debug.LogError(ex);
                AuthState = AuthState.Error;
            }
            catch(RequestFailedException requestEx)
            {
                Debug.LogError(requestEx);
                AuthState = AuthState.Error;
            }



            retries++;

            await Task.Delay(1000);
        }

        if (AuthState != AuthState.Authenticated)
        {
            Debug.LogWarning($"Player was not signed in successfully after {retries} retries!");
            AuthState = AuthState.Timeout;
        }

    }

    private static async Task<AuthState> Authenticating()
    {
        while(AuthState == AuthState.Authenticating || AuthState == AuthState.NotAuthenticated)
        {
            await Task.Delay(200);
        }

        return AuthState;
    }
    public static void ResetAuthState()
    {
        AuthState = AuthState.NotAuthenticated;
    }
}

public enum AuthState
{
    NotAuthenticated,
    Authenticating,
    Authenticated,
    Error,
    Timeout
}

