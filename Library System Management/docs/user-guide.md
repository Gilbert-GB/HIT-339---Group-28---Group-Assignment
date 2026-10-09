# User Guide

This guide explains how library patrons can search the catalogue, check item availability, place reservations and use the self-service kiosk. A short reference for staff features is included at the end.

The site is responsive and works on phones, tablets and desktop computers. No account or login is needed to search the catalogue.

---

## Part 1: For library patrons

### 1. Searching the catalogue

You can start a search in two ways:

- On the **Home** page, type into the search box and select **Search**.
- Select **Search** in the top navigation menu to open the full search page.

The search matches any part of an item's:

- title (for example, *Hobbit*)
- library code (for example, *B014*)
- author (for example, *Tolkien*)
- artist (for example, *Beatles*)
- description

Searches are not case-sensitive.

### 2. Filtering results

The search page has four filters. Set any combination, then select **Apply**.

| Filter | What it does |
|---|---|
| **Search** | Text to look for (see above). Leave it empty to list every item. |
| **Type** | Show only Books, Music or Toys. |
| **Status** | Show only items that are Available, Borrowed, Damaged or Destroyed. Choose **Available** to see what you can borrow today. |
| **Branch** | Show only items held at one library branch (Central Library, East Branch or West Branch). |

Select **Clear** to remove all filters and start again. The number of matching items is shown above the results.

On a phone, the filters appear above the results; scroll down to see the items.

### 3. Reading a search result

Each result card shows:

- the item's **title** and **library code**
- its **type** (Book, Music or Toy)
- its **status** (for example, Available or Borrowed)
- the **branch** where it is held, so you know where to collect it

Select **Details** to see the full description and other information about the item.

### 4. Reserving an item

If an item you want is currently **borrowed** or **damaged**, you can join its waiting list:

1. Open the item's **Details** page from your search results.
2. The page shows how many people are already waiting. Enter your email address and select **Place Hold**.
3. A message confirms your place in the queue.

Reservations are handled in the order they are placed. When the item becomes available, the first person on the list is sent a notification and has three days to collect it. If you are already on the list, the page shows your current position instead of adding you twice.

Items that are already available do not need a hold: the Details page shows which branch holds them, so you can borrow them in person or at a kiosk. Items that have been removed from circulation cannot be reserved.

### 5. Using the self-service kiosk

Kiosks inside the library let you check out items without visiting the front desk.

**To view your account:**

1. On the kiosk start screen, enter or scan your email address or library username, then select **Continue**.
2. Your account summary shows:
   - how many items you have on loan
   - any **overdue** items, highlighted in red, with the number of days late and the fine so far
   - fines currently accruing and fines already paid
   - your reservations and your place in each queue, or **Ready to collect** when an item is waiting for you
   - your five most recent returns

**To borrow an item:**

1. Open your account as above.
2. Scan the item's barcode or type its library code (for example, *B003*) into the **Borrow an item** box, then select **Checkout**.
3. A confirmation with the due date appears, and the item is added to your list. Items are borrowed for 14 days.

You can also select **Browse Items**, tap an available item, and enter your email or username to check it out.

**When you have finished**, select **Done**. For your privacy, the kiosk returns to the start screen automatically after 90 seconds without activity.

If the kiosk cannot find your account, please ask at the front desk to register.

### 6. Fines and reminders

- Items returned late are charged **$1.00 per day**.
- You will receive a reminder shortly before an item is due, and a message if an item becomes overdue.
- Notifications in this system are simulated: they are recorded in the staff notification log rather than sent to a real inbox or phone.

### 7. Paying fines

Fines for late returns can be paid at a kiosk:

1. Open your account at the kiosk. Any unpaid fines are listed under **Fines to pay**.
2. Select **Pay** next to the fine, enter your card details, and select **Pay** to confirm.
3. A confirmation with your receipt number appears, and an emailed receipt is recorded.

Fines on items that are still overdue keep increasing until the item is returned, so they can only be paid after the return.

Payments in this demonstration are simulated: no real payment is processed and card numbers are never stored.

---

## Part 2: Staff quick reference

Staff features require a login. Each role sees only its own areas.

| Area | Address | Roles | Purpose |
|---|---|---|---|
| Borrowing | Borrowing menu | Reception | Borrow and return items, manage borrower records |
| Item management | Admin menu | Admin | Create, edit and remove catalogue items |
| Branches | `/Branches` | Manager, Admin, Reception | View branches and transfer items between them |
| Import | `/Import` | Admin, Manager | Upload a CSV file of items (sample: `docs/sample-import.csv`), or look up items by ISBN, title or artist in an external catalogue |
| Notifications | `/Notifications` | Manager, Admin, Reception | Log of simulated email and SMS messages; **Run due-date check** creates reminders immediately |
| Manager dashboard | Manager menu | Manager | Statistics and four CSV reports: borrowing statistics, fine revenue audit, inventory health, items and borrow counts |

**Public API:** the `available` and `categories` endpoints require an `X-Api-Key` header; the `status` endpoint is public. See the installation guide for details.