# Anagram-Tree
A graph of anagrams of a base words

Words are connected when:
 - words only have letters from the base word
 - words have `n` and `n+1` letters respectively
 - words have exactly `n` letters in common


Example with base word `ювелирното`
![Example image](https://raw.githubusercontent.com/Mitiko/Anagram-Tree/master/wwwroot/example.png)

## Run the application
Just use `dotnet run`

## Install the database using docker
`docker run -itdp 5432:5432 mitiko/words-database`

## Notes
The database was downloaded from [Rechko](https://rechnik.chitanka.info/about)

The database simply consists of a lot of words in bulgarian

## How to convert to English
1. Find a big enough database with words
2. Export to `sql` and import in a `postgres` databse
3. To build a docker image for the database use the `database/Dockerfile`
4. The structure must be:
    - a table named `word`
    - table has columns `id` and `name`
5. Put all words in the the column `name`
6. Change the default connection string in `AdminContext.cs`
7. Change the alphabet dictionary