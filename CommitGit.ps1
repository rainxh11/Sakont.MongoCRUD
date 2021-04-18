$date = Get-Date
$commit = "Commit " + $date 

git add .
git commit -m $commit
git push -f 