"use client";

import { useEffect, useRef, useState } from "react";
import type { NotificationResponse } from "@/lib/types/NotificationResponse";

type NotificationListProps = {
  initialNotifications: NotificationResponse[];
};

const ReloadIntervalInMilliseconds = 30000;

// Shows the list the server rendered, then reloads it every 30 seconds. The interval is cleared when the page is left.
export function NotificationList(props: NotificationListProps) {
  const [notifications, setNotifications] = useState(
    props.initialNotifications,
  );
  const [errorMessage, setErrorMessage] = useState("");
  const intervalIdRef = useRef<number | null>(null);

  async function loadNotifications() {
    try {
      const response = await fetch("/api/notifications", { method: "GET" });
      if (response.ok === false) {
        setErrorMessage("The notifications could not be loaded.");
        return;
      }

      const loadedNotifications: NotificationResponse[] = await response.json();
      setNotifications(loadedNotifications);
      setErrorMessage("");
    } catch {
      setErrorMessage("The server could not be reached.");
    }
  }

  useEffect(function reloadNotificationsEveryThirtySeconds() {
    intervalIdRef.current = window.setInterval(
      loadNotifications,
      ReloadIntervalInMilliseconds,
    );

    return function stopReloadingWhenThePageIsLeft() {
      if (intervalIdRef.current !== null) {
        window.clearInterval(intervalIdRef.current);
      }
    };
  }, []);

  async function markAsRead(notificationId: number) {
    const response = await fetch(`/api/notifications/${notificationId}/read`, {
      method: "PATCH",
    });
    if (response.ok === false) {
      setErrorMessage("The notification could not be marked as read.");
      return;
    }

    await loadNotifications();
  }

  if (notifications.length === 0) {
    return <p>You have no notifications.</p>;
  }

  const rowElements: React.ReactElement[] = [];
  for (const notification of notifications) {
    rowElements.push(
      <NotificationRow
        key={notification.id}
        notification={notification}
        markAsRead={markAsRead}
      />,
    );
  }

  let errorElement = null;
  if (errorMessage !== "") {
    errorElement = <p className="text-red-700">{errorMessage}</p>;
  }

  return (
    <div className="flex flex-col gap-2">
      {errorElement}
      <table className="w-full border-collapse">
        <thead>
          <tr className="border-b border-gray-300 text-left">
            <th className="py-2">When</th>
            <th className="py-2">Type</th>
            <th className="py-2">Message</th>
            <th className="py-2"></th>
          </tr>
        </thead>
        <tbody>{rowElements}</tbody>
      </table>
    </div>
  );
}

type NotificationRowProps = {
  notification: NotificationResponse;
  markAsRead: (notificationId: number) => Promise<void>;
};

function NotificationRow(props: NotificationRowProps) {
  const notification = props.notification;
  const markAsRead = props.markAsRead;
  const createdAt = new Date(notification.createdAt);

  async function handleMarkAsReadButtonClick() {
    await markAsRead(notification.id);
  }

  let rowClassName = "border-b border-gray-200 font-bold";
  let actionElement = (
    <button
      type="button"
      onClick={handleMarkAsReadButtonClick}
      className="border border-gray-400 px-2 py-1 hover:bg-gray-100"
    >
      Mark as read
    </button>
  );
  if (notification.isRead) {
    rowClassName = "border-b border-gray-200 text-gray-600";
    actionElement = <span>Read</span>;
  }

  return (
    <tr className={rowClassName}>
      <td className="py-2">{createdAt.toLocaleString()}</td>
      <td className="py-2">{notification.type}</td>
      <td className="py-2">{notification.message}</td>
      <td className="py-2">{actionElement}</td>
    </tr>
  );
}
