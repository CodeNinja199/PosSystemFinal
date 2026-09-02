import { callPosApi } from "@/lib/callPosApi";
import { requireLoginToken } from "@/lib/requireLoginToken";
import type { NotificationResponse } from "@/lib/types/NotificationResponse";
import { NotificationList } from "@/app/components/NotificationList";

export default async function NotificationsPage() {
  const token = await requireLoginToken();

  const notificationsResponse = await callPosApi(
    "/notifications/notifications",
    { method: "GET", token: token, body: null },
  );
  const notifications: NotificationResponse[] =
    await notificationsResponse.json();

  return (
    <div>
      <h1 className="mb-4 text-2xl font-bold">Notifications</h1>
      <NotificationList initialNotifications={notifications} />
    </div>
  );
}
