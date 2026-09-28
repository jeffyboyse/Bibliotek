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

-- Dumpar struktur för tabell bibliotek.Author
DROP TABLE IF EXISTS `Author`;
CREATE TABLE IF NOT EXISTS `Author` (
  `Author_ID` int NOT NULL AUTO_INCREMENT,
  `First_name` varchar(50) NOT NULL DEFAULT '',
  `Last_name` varchar(50) NOT NULL,
  PRIMARY KEY (`Author_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Dumpar data för tabell bibliotek.Author: ~0 rows (ungefär)

-- Dumpar struktur för tabell bibliotek.Category
DROP TABLE IF EXISTS `Category`;
CREATE TABLE IF NOT EXISTS `Category` (
  `SAB_system` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Description` varchar(50) NOT NULL,
  PRIMARY KEY (`SAB_system`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Dumpar data för tabell bibliotek.Category: ~0 rows (ungefär)

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

-- Dumpar data för tabell bibliotek.Copy: ~0 rows (ungefär)

-- Dumpar struktur för tabell bibliotek.Invoice
DROP TABLE IF EXISTS `Invoice`;
CREATE TABLE IF NOT EXISTS `Invoice` (
  `InvoiceID` int NOT NULL AUTO_INCREMENT,
  `Paid_date` date NOT NULL,
  `Last_due_date` date NOT NULL,
  `Created_date` date NOT NULL,
  `Amount` varchar(50) NOT NULL DEFAULT '0',
  `LoanID` int NOT NULL,
  PRIMARY KEY (`InvoiceID`),
  KEY `FK_Invoice_Loan` (`LoanID`),
  CONSTRAINT `FK_Invoice_Loan` FOREIGN KEY (`LoanID`) REFERENCES `Loan` (`LoanID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Dumpar data för tabell bibliotek.Invoice: ~0 rows (ungefär)

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
  CONSTRAINT `FK_Loan_User` FOREIGN KEY (`UserID`) REFERENCES `User` (`Userr_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Dumpar data för tabell bibliotek.Loan: ~0 rows (ungefär)

-- Dumpar struktur för tabell bibliotek.Media
DROP TABLE IF EXISTS `Media`;
CREATE TABLE IF NOT EXISTS `Media` (
  `Media_ID` int NOT NULL AUTO_INCREMENT,
  `Type` enum('book','audiobook','movie') CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT 'book' COMMENT 'Sätts till ''book'', ''audiobook'', ''movie''',
  `Title` varchar(50) NOT NULL DEFAULT '',
  `Replacement_value` decimal(20,6) NOT NULL DEFAULT (0) COMMENT 'Ersättningsvärde i kr',
  `SAB_system` varchar(50) NOT NULL DEFAULT '',
  `ISBN` varchar(50) DEFAULT NULL COMMENT 'Fylls endast in om type = ''book'' eller ''audiobook''',
  `EAN` varchar(50) DEFAULT NULL COMMENT 'Fylls endast in om type = ''movie''',
  PRIMARY KEY (`Media_ID`),
  KEY `FK_Media_Category` (`SAB_system`),
  CONSTRAINT `FK_Media_Category` FOREIGN KEY (`SAB_system`) REFERENCES `Category` (`SAB_system`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Dumpar data för tabell bibliotek.Media: ~0 rows (ungefär)

-- Dumpar struktur för tabell bibliotek.MediaAuthor
DROP TABLE IF EXISTS `MediaAuthor`;
CREATE TABLE IF NOT EXISTS `MediaAuthor` (
  `MediaAuthorID` int NOT NULL AUTO_INCREMENT,
  `AuthorID` int NOT NULL,
  `MediaID` int NOT NULL,
  PRIMARY KEY (`MediaAuthorID`),
  KEY `FK_MediaAuthor_Author` (`AuthorID`),
  KEY `FK_MediaAuthor_Media` (`MediaID`),
  CONSTRAINT `FK_MediaAuthor_Author` FOREIGN KEY (`AuthorID`) REFERENCES `Author` (`Author_ID`),
  CONSTRAINT `FK_MediaAuthor_Media` FOREIGN KEY (`MediaID`) REFERENCES `Media` (`Media_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Dumpar data för tabell bibliotek.MediaAuthor: ~0 rows (ungefär)

-- Dumpar struktur för tabell bibliotek.User
DROP TABLE IF EXISTS `User`;
CREATE TABLE IF NOT EXISTS `User` (
  `Userr_ID` int NOT NULL AUTO_INCREMENT,
  `Username` varchar(50) NOT NULL,
  `Email` varchar(50) NOT NULL,
  `Password` varchar(50) NOT NULL,
  `Role` tinyint(1) NOT NULL DEFAULT (0),
  PRIMARY KEY (`Userr_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Role is to see if the user is either admin or a normal user that is there to borrow books\r\n';

-- Dumpar data för tabell bibliotek.User: ~0 rows (ungefär)

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
