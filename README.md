## Case Study

AIT Research (AITR) is a market research company that allows people from the general public to register their details, buying habits, etc. with AITR, and then sends these respondents to market research jobs, based on the needs of AITR's clients.

AITR asks respondents to email their details and maintains these manual emails. To find all respondents who match the client's needs, staff at AITR look through the emails to find respondents who match the criteria – for example, Males between the ages of 24 to 29 who are using ANZ bank, interested in Horse Racing Sport and Asia travel. Or, Females who use Westpac as one of their banks.

AITR wants to move to a web-based system where respondents can register their details online, and staff can then search through these registrations to find respondents who match the requested criteria.

## User Requirements

The requirements for the new system are as follows:

- The user interface for respondents and staff should be Web-based
- Staff should be authenticated with a username and password
- Once logged in, staff can:
  - View a list of all respondents
  - View a list of respondents that match certain criteria, including any of the below:
    - First Name
    - Last Name
    - Age Range
    - State/Territory
    - Gender
    - Home Suburb
    - Home Postcode
    - Email Address
    - Banks Used (e.g. search for all respondents who use Westpac) ** This is compulsory
    - Banks Service (e.g. search for all respondents who have a mortgage) ** This is compulsory
    - Newspaper Read (e.g. search for all respondents who read The Daily Telegraph)
    - Etc.
  - The staff member should be able to enter as many or as few search criteria as they want. After clicking search, they should see a grid below their search criteria containing a list of all matching respondents, in ascending order by surname.

- The system should record when a respondent has attended a market research session with the following details:
  - Date
  - IP Address

- Respondents can attend by accessing a website, and are not required to log in or authenticate their identity

- The questions they are asked to answer include:
  - Gender
  - Age range
  - State or Territory of Australia
  - Home Suburb and Postcode
  - Email Address
  - Banks Used (maximum of 4)
  - Newspaper Read (maximum of 2)

- If respondents would like to register as members of this program, the system should ask for the following information; otherwise, their name will be recorded as Anonymous
  - Given Names + Last Name
  - Date of Birth
  - Contact Phone Number

- If respondents use Commonwealth Bank, Wespac or ANZ, the system should ask for additional services like:
  - Internet Banking
  - Home Loan
  - Credit card
  - Share Investment

- For respondents who read a newspaper, the system should collect additional interested news sections like:
  - Property
  - Sport
  - Financial
  - Entertainment
  - Lifestyle
  - Travel
  - Politics

- For respondents who are interested in sports, the system should ask for the type of sport, like:
  - AFL
  - Football
  - Cricket
  - Racing
  - Motorsport
  - Basketball
  - Tennis

- For respondents who are interested in travel, the system should ask for a destination, which includes the following locations (as a minimum 2 locations):
  - Australia
  - Europe
  - Pacific
  - North America
  - South America
  - Asia
  - Middle East
  - Africa

- Respondents should not need to manually type in their State, bank information or newspaper information – they should choose them from a logical user interface.

## Out-of-Scope Requirements

The below elements do not need to be included. Students should assume that the following items already exist:

- An interface to create and edit usernames and passwords (Instead, students should manually create a database of users sufficient to demonstrate and test the system).
- A user interface to maintain reference data like Banks, Newspapers etc (Instead, students will simply create test data in their databases).
- An installer program.
