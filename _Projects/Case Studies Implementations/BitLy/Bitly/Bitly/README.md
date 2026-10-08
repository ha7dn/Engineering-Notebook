## Description
An URL shortening service that converts long URLs into short, manageable links.
It also provides analytics for the shortened URLs.

## Functional Requirements
- [ ] Users should be able to submit a long URL and receive a shortened version
- [ ] Users should be able to access the original URL by using the shortened one
- [ ] Users should be able to specify a custom alias
- [ ] Users should be able to specify an expiration date for their shortened URLs

**Out of Scope**
- User auth and account management
- Analytics on link clicks

## Non-functional Requirements
- [ ] The system should be ensure uniqueness for the short codes
	- [ ] Each code should map to exactly one long URL
- [ ] The redirection should happen awith minimal delay (<100ms)
- [ ] The system should be reliable and available 99.9% of the time (avalilability > consistency)
- [ ] The system should scale to support up to 1B shortened URLs and 100M Daily Active Users

**Out of scope**
- Data consistency in real-time analytics
- Advanced security features like spam detection and malicious URL filtering
