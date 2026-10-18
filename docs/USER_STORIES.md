# HireNow · Product vision and user stories

## Product goal

HireNow is a digital marketplace that connects freelance service providers with new clients. It gives independent contractors a place to present and sell their services, and gives people looking for a service a fast way to find, contact and safely pay the right contractor.

## Users

| Role | Who | Main goal |
|---|---|---|
| **Service provider (SP)**, seller | Freelancers and independent contractors | Present their services, win clients and get paid |
| **Service seeker (SS)**, buyer | Individuals or small businesses that need a service done | Find the right provider, agree on the work and pay safely |
| **Admin** | The HireNow team | Keep the marketplace safe, healthy and measurable |

## User stories

These are the original stories with stable IDs. The refined, estimated backlog items that implement them (`HN-xx`) are in [BACKLOG.md](BACKLOG.md).

### Service providers (sellers)

| ID | User story | Backlog items |
|---|---|---|
| SP-01 | As a service provider, I want to fully present my service to potential clients through listings, using text, photos and videos. | HN-07, HN-08, HN-11 |
| SP-02 | As a SP, I want to publish my services with my desired price range and create invoices upon client agreement. | HN-07, HN-24 |
| SP-03 | As a SP, I want all my services listed under my profile so clients can find them easily. | HN-06, HN-10 |
| SP-04 | As a SP, I want my Terms of Service to be easy for clients to read. | HN-12 |
| SP-05 | As a SP, I want to be able to send a contract to the client. | HN-25 |
| SP-06 | As a SP, I want to edit my services if something about them changes. | HN-09 |
| SP-07 | As a SP, I want to manage client requests for my services. | HN-16, HN-17 |
| SP-08 | As a SP, I want to communicate with potential clients before and/or after they pay for my services. | HN-27 |
| SP-09 | As a SP, I want clients to find my services through keywords and/or filters. | HN-13 |
| SP-10 | As a SP, I want different payment options (when and how the payment is made). | HN-20, HN-21, HN-23 |
| SP-11 | As a SP, I want my clients to leave reviews on my services that other people can see. | HN-28 |
| SP-12 | As a SP, I want to review my clients so other SPs understand who they might be working with. | HN-29 |

### Service seekers (buyers)

| ID | User story | Backlog items |
|---|---|---|
| SS-01 | As a service seeker, I want to easily find the service I am looking for. | HN-13 |
| SS-02 | As a SS, I want to save services somewhere I can easily access them. | HN-14 |
| SS-03 | As a SS, I want to cancel my orders within a period of time. | HN-16, HN-19 |
| SS-04 | As a SS, I want to be able to sign a contract when I'm given one. | HN-26 |
| SS-05 | As a SS, I want to leave reviews on the services I've used. | HN-28 |
| SS-06 | As a SS, I want to see a history of the services I've ordered. | HN-18 |
| SS-07 | As a SS, I want to follow SP pages to keep up with them. | HN-15, HN-33 |
| SS-08 | As a SS, I want my payment to not reach the SP until proof of completion is provided. | HN-21, HN-22 |

### Admin

| ID | User story | Backlog items |
|---|---|---|
| AD-01 | As an admin, I want to manage all users and the services they post. | HN-30 |
| AD-02 | As an admin, I want to receive statistics about the app. | HN-31 |
| AD-03 | As an admin, I want feedback and complaints from SPs and SSs to come into my inbox so I can review them. | HN-32 |

## What changed during refinement

- **Merged overlaps.** SP-09 and SS-01 describe the same search feature from two sides, so both are covered by HN-13. SP-11 and SS-05 are both about service reviews, covered by HN-28.
- **Split large stories.** SP-01 became text and price (HN-07), photos (HN-08) and video (HN-11). SS-08 became paying with the money held (HN-21) and releasing it on completion (HN-22). SP-02 became the listing (HN-07) and the invoice (HN-24).
- **Added missing stories.** Every story needs accounts, so we added registration and sign-in (HN-05) and profiles (HN-06). SP-07 needs a buyer to send a request first (HN-16). Technical enablers (HN-01 to HN-04) set up the code, CI, the cloud and deployment.

## Assumptions to confirm with the product owner

Each of these shaped an acceptance criterion. If one is wrong, the linked item changes.

1. **SP-04 Terms of Service.** Each seller writes their own terms, shown on their profile and service pages and accepted when a client sends a request. There is also one static platform-wide Terms page. (HN-12)
2. **SS-08 payment held until completion.** The original story was marked "(>?)". We treat it as a Must because it decides how payments work: the buyer pays HireNow, the money is held, and it goes to the seller when the buyer approves the delivery, or automatically 3 days after delivery. (HN-21, HN-22)
3. **SS-03 cancellation window.** A buyer can withdraw a request at any time before paying. After paying, they can cancel within 24 hours, as long as the work hasn't been delivered, and get a full refund. (HN-16, HN-19)
4. **SP-10 payment options.** "How" means the methods Stripe Checkout offers (card, Apple Pay, Google Pay). "When" means full payment upfront (MVP) or a deposit plus the balance on delivery (Could). (HN-21, HN-23)
5. **SP-12 client reviews.** Only sellers and the reviewed buyer can see them. They are never public. (HN-29)
6. **SS-04 contracts.** Click-to-sign: typed name, timestamp and IP address. This is not a legally binding e-signature. (HN-26)
7. **Platform fee.** HireNow keeps a configurable commission, 10% by default, when the money is released to the seller. (HN-22)
8. **One role per account.** A user registers as either a seller or a buyer. Allowing one account to be both can come later. (HN-05)
9. **Single currency and language.** One platform currency (EUR) and English only for this release.
