using CareerBridge.Shared;

namespace CareerBridge.Web.Services;

public class CurrentUserState
{
    public UserProfile? Current { get; private set; }

    public event Action? Changed;

    public void SetUser(UserProfile? user)
    {
        Current = user;
        Changed?.Invoke();
    }
}
