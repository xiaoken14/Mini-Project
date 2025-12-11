# Silver & Scholar Health Scheduler

A clinic consultation system.

## Database Schema

### Table 1: Admin

| Field Name      | Data Type    | Key | Description                        |
| --------------- | ------------ | --- | ---------------------------------- |
| Admin_ID        | INT          | PK  | Unique identifier for each admin user |
| Full_Name       | VARCHAR(100) | –   | Admin’s full name                  |
| Email           | VARCHAR(100) | –   | Admin login email                  |
| Password        | VARCHAR(255) | –   | Hashed password for login          |
| Contact_No      | VARCHAR(20)  | –   | Admin’s phone number               |
| Date_Created    | DATETIME     | –   | Date the admin account was created |

### Table 2: Doctor

| Field Name        | Data Type    | Key | Description                                |
| ----------------- | ------------ | --- | ------------------------------------------ |
| Doctor_ID         | INT          | PK  | Unique identifier for each doctor          |
| Full_Name         | VARCHAR(100) | –   | Doctor’s full name                         |
| Email             | VARCHAR(100) | –   | Doctor’s login email                       |
| Password          | VARCHAR(255) | –   | Hashed password                            |
| Specialization    | VARCHAR(100) | –   | Doctor’s medical specialty                 |
| Consultation_Hours| VARCHAR(50)  | –   | Available working hours                    |
| Status            | VARCHAR(20)  | –   | Active / On Leave                          |
| Admin_ID          | INT          | FK  | References Admin.Admin_ID (assigned by admin) |

### Table 3: Patient

| Field Name           | Data Type    | Key | Description                        |
| -------------------- | ------------ | --- | ---------------------------------- |
| Patient_ID           | INT          | PK  | Unique patient ID                  |
| Full_Name            | VARCHAR(100) | –   | Patient’s full name                |
| Email                | VARCHAR(100) | –   | Patient login email                |
| Password             | VARCHAR(255) | –   | Hashed password                    |
| Contact_No           | VARCHAR(20)  | –   | Patient phone number               |
| Age                  | INT          | –   | Patient age                        |
| Category             | VARCHAR(20)  | –   | Student / Senior / Regular         |
| Discount_Eligibility | VARCHAR(10)  | –   | Yes / No                           |
| Admin_ID             | INT          | FK  | References Admin.Admin_ID          |

### Table 4: Appointment

| Field Name        | Data Type   | Key | Description                             |
| ----------------- | ----------- | --- | --------------------------------------- |
| Appointment_ID    | INT         | PK  | Unique appointment ID                   |
| Admin_ID          | INT         | FK  | References Admin.Admin_ID             |
| Doctor_ID         | INT         | FK  | References Doctor.Doctor_ID           |
| Patient_ID        | INT         | FK  | References Patient.Patient_ID         |
| Appointment_Date  | DATE        | –   | Appointment date                        |
| Appointment_Time  | TIME        | –   | Appointment time                        |
| Status            | VARCHAR(20) | –   | Booked / Cancelled / Completed          |
| Priority          | VARCHAR(20) | –   | Normal / Student / Senior               |
| Discount_Applied  | VARCHAR(10) | –   | Yes / No                                |

### Table 5: Notification

| Field Name          | Data Type   | Key | Description                               |
| ------------------- | ----------- | --- | ----------------------------------------- |
| Notification_ID     | INT         | PK  | Unique notification ID                    |
| Appointment_ID      | INT         | FK  | References Appointment.Appointment_ID   |
| Message             | TEXT        | –   | Notification message content              |
| Notification_DateTime| DATETIME    | –   | Date and time notification was sent       |
| Type                | VARCHAR(30) | –   | Confirmation / Reminder / Cancellation    |

### Table 6: Report

| Field Name     | Data Type    | Key | Description                 |
| -------------- | ------------ | --- | --------------------------- |
| Report_ID      | INT          | PK  | Unique report ID            |
| Admin_ID       | INT          | FK  | References Admin.Admin_ID |
| Report_Type    | VARCHAR(50)  | –   | Type of report generated    |
| Generated_Date | DATETIME     | –   | Date report was created     |
| File_Path      | VARCHAR(255) | –   | File storage location       |

### Table 7: Payment

| Field Name     | Data Type     | Key | Description                             |
| -------------- | ------------- | --- | --------------------------------------- |
| Payment_ID     | INT           | PK  | Unique payment ID                       |
| Appointment_ID | INT           | FK  | References Appointment.Appointment_ID |
| Amount         | DECIMAL(10,2) | –   | Final payment amount                    |
| Payment_Date   | DATETIME      | –   | Date payment was made                   |
| Payment_Status | VARCHAR(20)   | –   | Pending / Completed / Failed            |
| Payment_Method | VARCHAR(20)   | –   | Cash / Online                           |
