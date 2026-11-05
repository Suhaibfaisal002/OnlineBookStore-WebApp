using Microsoft.AspNetCore.Http;

namespace Online_Booking_Store.Infrastructure;

public static class SessionExtensions
{
    public static string GetOrCreateGuestSessionId(this ISession session)
    {
        var sessionId = session.GetString(SessionKeys.GuestSessionId);
        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            return sessionId;
        }

        sessionId = Guid.NewGuid().ToString("N");
        session.SetString(SessionKeys.GuestSessionId, sessionId);
        return sessionId;
    }
}
