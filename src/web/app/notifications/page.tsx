"use client";

import { useEffect, useState } from "react";
import { readFromApi } from "@/lib/callWebApi";
import { useRequireLogin } from "@/lib/useRequireLogin";
import type { NotificationResponse } from "@/lib/types/NotificationResponse";
import { NotificationList } from "@/app/components/NotificationList";

// A client component because the token lives in localStorage, which only the browser can read.
// The first list is fetched here and handed to NotificationList as its starting point, exactly as
// the server render used to, so the 30-second reload inside that component is untouched.
export default function NotificationsPage() {
  const { isReady } = useRequireLogin(null);
  const [notifications, setNotifications] = useState<
    NotificationResponse[] | null
  >(null);
  const [errorMessage, setErrorMessage] = useState("");

  useEffect(
    function loadNotifications() {
      if (isReady === false) {
        return;
      }

      readFromApi<NotificationResponse[]>("/notifications/notifications")
        .then(function showThem(loadedNotifications) {
          setNotifications(loadedNotifications);
        })
        .catch(function showTheProblem() {
          setErrorMessage("The notifications could not be loaded.");
        });
    },
    [isReady],
  );

  // Nothing of this page is drawn once the stored login has gone: the redirect from useRequireLogin
  // is already on its way, and what was fetched belonged to whoever was signed in a moment ago.
  if (isReady === false) {
    return <p>Loading…</p>;
  }

  if (errorMessage !== "") {
    return <p className="text-red-700">{errorMessage}</p>;
  }

  if (notifications === null) {
    return <p>Loading…</p>;
  }

  return (
    <div>
      <h1 className="mb-4 text-2xl font-bold">Notifications</h1>
      <NotificationList initialNotifications={notifications} />
    </div>
  );
}
