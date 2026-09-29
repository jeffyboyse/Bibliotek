-- --------------------------------------------------------
-- Värd:                         127.0.0.1
-- Serverversion:                8.0.46 - MySQL Community Server - GPL
-- Server-OS:                    Linux
-- HeidiSQL Version:             12.21.0.7344
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;


-- Dumpar databasstruktur för bibliotek
CREATE DATABASE IF NOT EXISTS `bibliotek` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `bibliotek`;

-- Dumpar struktur för tabell bibliotek.Attribute
DROP TABLE IF EXISTS `Attribute`;
CREATE TABLE IF NOT EXISTS `Attribute` (
  `AttributeID` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT '0',
  PRIMARY KEY (`AttributeID`)
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Dumpar data för tabell bibliotek.Attribute: ~10 rows (ungefär)
INSERT INTO `Attribute` (`AttributeID`, `Name`) VALUES
	(1, 'Director'),
	(2, 'Narrator'),
	(3, 'ISBN'),
	(4, 'Runtime'),
	(5, 'Genre'),
	(6, 'Publisher'),
	(7, 'Language'),
	(8, 'AgeRating'),
	(9, 'EAN'),
	(10, 'PageCount');

-- Dumpar struktur för tabell bibliotek.Author
DROP TABLE IF EXISTS `Author`;
CREATE TABLE IF NOT EXISTS `Author` (
  `AuthorID` int NOT NULL,
  `First_Name` varchar(50) NOT NULL DEFAULT '',
  `Last_Name` varchar(50) NOT NULL DEFAULT '',
  PRIMARY KEY (`AuthorID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Dumpar data för tabell bibliotek.Author: ~6 rows (ungefär)
INSERT INTO `Author` (`AuthorID`, `First_Name`, `Last_Name`) VALUES
	(1, 'J.R.R.', 'Tolkien'),
	(2, 'George', 'Orwell'),
	(3, 'Michelle', 'Obama'),
	(4, 'Frank', 'Herbert'),
	(5, 'J.K.', 'Rowling'),
	(6, 'Harper', 'Lee');

-- Dumpar struktur för tabell bibliotek.Category
DROP TABLE IF EXISTS `Category`;
CREATE TABLE IF NOT EXISTS `Category` (
  `SAB_system` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Description` varchar(50) NOT NULL,
  PRIMARY KEY (`SAB_system`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Dumpar data för tabell bibliotek.Category: ~8 rows (ungefär)
INSERT INTO `Category` (`SAB_system`, `Description`) VALUES
	('Hc', 'Svenska romaner'),
	('Hcb', 'Ungdomslitteratur'),
	('Hcg', 'Barnlitteratur'),
	('He', 'Engelsk skönlitteratur'),
	('I', 'Konst, musik och film'),
	('O', 'Samhälls- och rättsvetenskap'),
	('T', 'Teknik och datavetenskap'),
	('U', 'Naturvetenskap');

-- Dumpar struktur för tabell bibliotek.Copy
DROP TABLE IF EXISTS `Copy`;
CREATE TABLE IF NOT EXISTS `Copy` (
  `Bar_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Status` tinyint(1) NOT NULL COMMENT 'Status är för att kolla om boken har blivit utlånad eller inte',
  `LoanID` int NOT NULL,
  PRIMARY KEY (`Bar_code`) USING BTREE,
  KEY `FK_Copy_Loan` (`LoanID`),
  CONSTRAINT `FK_Copy_Loan` FOREIGN KEY (`LoanID`) REFERENCES `Loan` (`LoanID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Dumpar data för tabell bibliotek.Copy: ~8 rows (ungefär)
INSERT INTO `Copy` (`Bar_code`, `Status`, `LoanID`) VALUES
	('BC001', 0, 1),
	('BC002', 1, 2),
	('BC003', 0, 3),
	('BC004', 1, 4),
	('BC005', 0, 5),
	('BC006', 0, 6),
	('BC007', 1, 7),
	('BC008', 0, 8);

-- Dumpar struktur för tabell bibliotek.Invoice
DROP TABLE IF EXISTS `Invoice`;
CREATE TABLE IF NOT EXISTS `Invoice` (
  `InvoiceID` int NOT NULL AUTO_INCREMENT,
  `Paid_date` date NOT NULL,
  `Invoice_paid` tinyint(1) NOT NULL DEFAULT (0),
  `Last_due_date` date NOT NULL,
  `Created_date` date NOT NULL,
  `Amount` varchar(50) NOT NULL DEFAULT '0',
  `LoanID` int NOT NULL,
  PRIMARY KEY (`InvoiceID`),
  KEY `FK_Invoice_Loan` (`LoanID`),
  CONSTRAINT `FK_Invoice_Loan` FOREIGN KEY (`LoanID`) REFERENCES `Loan` (`LoanID`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Dumpar data för tabell bibliotek.Invoice: ~5 rows (ungefär)
INSERT INTO `Invoice` (`InvoiceID`, `Paid_date`, `Invoice_paid`, `Last_due_date`, `Created_date`, `Amount`, `LoanID`) VALUES
	(1, '2026-03-01', 0, '2026-03-15', '2026-02-15', '100', 1),
	(2, '2026-02-15', 0, '2026-03-01', '2026-02-01', '150', 2),
	(3, '2026-02-01', 0, '2026-02-20', '2026-01-20', '50', 3),
	(4, '2026-02-15', 0, '2026-03-05', '2026-02-05', '50', 4),
	(5, '2026-03-15', 0, '2026-04-01', '2026-03-01', '200', 5);

-- Dumpar struktur för tabell bibliotek.Loan
DROP TABLE IF EXISTS `Loan`;
CREATE TABLE IF NOT EXISTS `Loan` (
  `LoanID` int NOT NULL AUTO_INCREMENT,
  `Return_date` date NOT NULL,
  `Loaning_date` date NOT NULL,
  `UserID` int NOT NULL DEFAULT '0',
  `LastReturn_date` date NOT NULL,
  `Bar_code` varchar(50) NOT NULL DEFAULT '',
  PRIMARY KEY (`LoanID`),
  KEY `FK_Loan_User` (`UserID`),
  KEY `FK_Loan_Copy` (`Bar_code`),
  CONSTRAINT `FK_Loan_Copy` FOREIGN KEY (`Bar_code`) REFERENCES `Copy` (`Bar_code`),
  CONSTRAINT `FK_Loan_User` FOREIGN KEY (`UserID`) REFERENCES `User` (`User_ID`)
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Dumpar data för tabell bibliotek.Loan: ~8 rows (ungefär)
INSERT INTO `Loan` (`LoanID`, `Return_date`, `Loaning_date`, `UserID`, `LastReturn_date`, `Bar_code`) VALUES
	(1, '2026-03-01', '2026-02-01', 3, '2026-03-01', 'BC001'),
	(2, '2026-02-15', '2026-01-15', 4, '2026-02-15', 'BC002'),
	(3, '2026-03-10', '2026-02-10', 5, '2026-03-10', 'BC003'),
	(4, '2026-02-01', '2026-01-01', 6, '2026-02-01', 'BC004'),
	(5, '2026-03-20', '2026-02-20', 7, '2026-03-20', 'BC005'),
	(6, '2026-01-30', '2026-01-01', 3, '2026-01-25', 'BC006'),
	(7, '2026-02-05', '2026-01-05', 4, '2026-02-02', 'BC007'),
	(8, '2026-02-10', '2026-01-10', 5, '2026-02-08', 'BC008');

-- Dumpar struktur för tabell bibliotek.Media
DROP TABLE IF EXISTS `Media`;
CREATE TABLE IF NOT EXISTS `Media` (
  `MediaID` int NOT NULL AUTO_INCREMENT,
  `Title` varchar(50) NOT NULL DEFAULT '',
  `Publish_year` int NOT NULL DEFAULT (0),
  PRIMARY KEY (`MediaID`)
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Dumpar data för tabell bibliotek.Media: ~10 rows (ungefär)
INSERT INTO `Media` (`MediaID`, `Title`, `Publish_year`) VALUES
	(1, 'The Hobbit', 1937),
	(2, 'The Fellowship of the Ring', 2021),
	(3, 'Inception', 2010),
	(4, '1984', 1949),
	(5, 'Becoming', 2018),
	(6, 'Interstellar', 2014),
	(7, 'Dune', 1965),
	(8, 'Harry Potter and the Philosopher\'s Stone', 2015),
	(9, 'The Matrix', 1999),
	(10, 'To Kill a Mockingbird', 1960);

-- Dumpar struktur för tabell bibliotek.MediaAttribute
DROP TABLE IF EXISTS `MediaAttribute`;
CREATE TABLE IF NOT EXISTS `MediaAttribute` (
  `MediaID` int NOT NULL,
  `Value` varchar(50) NOT NULL DEFAULT '',
  `AttributeID` int NOT NULL,
  KEY `FK_MediaAttribute_Media` (`MediaID`),
  KEY `FK_MediaAttribute_Attribute` (`AttributeID`),
  CONSTRAINT `FK_MediaAttribute_Attribute` FOREIGN KEY (`AttributeID`) REFERENCES `Attribute` (`AttributeID`),
  CONSTRAINT `FK_MediaAttribute_Media` FOREIGN KEY (`MediaID`) REFERENCES `Media` (`MediaID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Dumpar data för tabell bibliotek.MediaAttribute: ~66 rows (ungefär)
INSERT INTO `MediaAttribute` (`MediaID`, `Value`, `AttributeID`) VALUES
	(1, '9780261102217', 3),
	(1, 'Fantasy', 5),
	(1, 'George Allen & Unwin', 6),
	(1, 'English', 7),
	(1, '9780261102217', 9),
	(1, '310', 10),
	(2, 'Andy Serkis', 2),
	(2, '9780063221192', 3),
	(2, '11:43:00', 4),
	(2, 'Fantasy', 5),
	(2, 'HarperCollins', 6),
	(2, 'English', 7),
	(2, '9780063221192', 9),
	(3, 'Christopher Nolan', 1),
	(3, '02:28:00', 4),
	(3, 'Sci-Fi', 5),
	(3, 'Warner Bros.', 6),
	(3, 'English', 7),
	(3, 'PG-13', 8),
	(3, '5051888049732', 9),
	(4, '9780451524935', 3),
	(4, 'Dystopian', 5),
	(4, 'Secker & Warburg', 6),
	(4, 'English', 7),
	(4, '9780451524935', 9),
	(4, '328', 10),
	(5, 'Michelle Obama', 2),
	(5, '9780525633686', 3),
	(5, '19:03:00', 4),
	(5, 'Biography', 5),
	(5, 'Random House Audio', 6),
	(5, 'English', 7),
	(5, '9780525633686', 9),
	(6, 'Christopher Nolan', 1),
	(6, '02:49:00', 4),
	(6, 'Sci-Fi', 5),
	(6, 'Paramount Pictures', 6),
	(6, 'English', 7),
	(6, 'PG-13', 8),
	(6, '5053083021931', 9),
	(7, '9780441172719', 3),
	(7, 'Sci-Fi', 5),
	(7, 'Chilton Books', 6),
	(7, 'English', 7),
	(7, '9780441172719', 9),
	(7, '412', 10),
	(8, 'Stephen Fry', 2),
	(8, '9781781102367', 3),
	(8, '08:25:00', 4),
	(8, 'Fantasy', 5),
	(8, 'Pottermore Publishing', 6),
	(8, 'English', 7),
	(8, '9781781102367', 9),
	(9, 'Lana Wachowski, Lilly Wachowski', 1),
	(9, '02:16:00', 4),
	(9, 'Sci-Fi', 5),
	(9, 'Warner Bros.', 6),
	(9, 'English', 7),
	(9, 'R', 8),
	(9, '7321900161830', 9),
	(10, '9780060935467', 3),
	(10, 'Classic Literature', 5),
	(10, 'J. B. Lippincott & Co.', 6),
	(10, 'English', 7),
	(10, '9780060935467', 9),
	(10, '281', 10);

-- Dumpar struktur för tabell bibliotek.MediaAuthor
DROP TABLE IF EXISTS `MediaAuthor`;
CREATE TABLE IF NOT EXISTS `MediaAuthor` (
  `MediaID` int NOT NULL,
  `AuthorID` int NOT NULL,
  `MediaAuthorID` int NOT NULL AUTO_INCREMENT,
  PRIMARY KEY (`MediaAuthorID`),
  KEY `FK_MediaAuthor_Author` (`AuthorID`),
  KEY `FK_MediaAuthor_Media` (`MediaID`),
  CONSTRAINT `FK_MediaAuthor_Author` FOREIGN KEY (`AuthorID`) REFERENCES `Author` (`AuthorID`),
  CONSTRAINT `FK_MediaAuthor_Media` FOREIGN KEY (`MediaID`) REFERENCES `Media` (`MediaID`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Dumpar data för tabell bibliotek.MediaAuthor: ~7 rows (ungefär)
INSERT INTO `MediaAuthor` (`MediaID`, `AuthorID`, `MediaAuthorID`) VALUES
	(1, 1, 1),
	(1, 2, 2),
	(2, 4, 3),
	(3, 5, 4),
	(4, 7, 5),
	(5, 8, 6),
	(6, 10, 7);

-- Dumpar struktur för tabell bibliotek.User
DROP TABLE IF EXISTS `User`;
CREATE TABLE IF NOT EXISTS `User` (
  `User_ID` int NOT NULL AUTO_INCREMENT,
  `FirstName` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `LastName` varchar(50) NOT NULL,
  `Email` varchar(50) NOT NULL,
  `Password` varchar(50) NOT NULL,
  `Role` tinyint(1) NOT NULL DEFAULT (0),
  PRIMARY KEY (`User_ID`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Role is to see if the user is either admin or a normal user that is there to borrow books\r\n';

-- Dumpar data för tabell bibliotek.User: ~0 rows (ungefär)
INSERT INTO `User` (`User_ID`, `FirstName`, `LastName`, `Email`, `Password`, `Role`) VALUES
	(1, 'John', 'Doe', 'john.doe@example.com', 'hashed_pass_1', 1),
	(2, 'Jane', 'Smith', 'jane.smith@example.com', 'hashed_pass_2', 0),
	(3, 'Alex', 'Miller', 'alex.miller@example.com', 'hashed_pass_3', 0),
	(4, 'Emily', 'Stone', 'emily.stone@example.com', 'hashed_pass_4', 0),
	(5, 'Michael', 'Brown', 'michael.brown@example.com', 'hashed_pass_5', 1),
	(6, 'Sarah', 'Wilson', 'sarah.wilson@example.com', 'hashed_pass_6', 0),
	(7, 'David', 'King', 'david.king@example.com', 'hashed_pass_7', 0),
	(8, 'Laura', 'Taylor', 'laura.taylor@example.com', 'hashed_pass_8', 0),
	(9, 'Chris', 'Parker', 'chris.parker@example.com', 'hashed_pass_9', 0),
	(10, 'Anna', 'Baker', 'anna.baker@example.com', 'hashed_pass_10', 0);

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
