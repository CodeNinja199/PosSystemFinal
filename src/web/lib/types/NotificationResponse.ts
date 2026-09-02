// One notification as the Notification API returns it.
export type NotificationResponse = {
  id: number;
  type: string;
  message: string;
  createdAt: string;
  isRead: boolean;
};
