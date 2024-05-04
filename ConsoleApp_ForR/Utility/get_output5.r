args <- commandArgs(trailingOnly = TRUE)
file_path <- args[1]
print(args)

if (!file.exists(file_path)) {
    stop(paste("The file does not exist:", file_path))
}

data <- tryCatch({
    read.csv(file_path, header = TRUE)
}, error = function(e) {
    stop(paste("Failed to read the file:", e$message))
})

# Calculate various statistical measures
if ("Data" %in% names(data)) {
  sigma <- sum(data$Data, na.rm = TRUE)
  xbar <- mean(data$Data, na.rm = TRUE)  
  stddev <- sd(data$Data, na.rm = TRUE)

# Print results
cat("***    RESULTS      ***\n")
cat("-----------------------\n")
cat("Statistic\tValue\n")
cat("Sum\t\t", sprintf("%.2f", sigma), "\n", sep="")
cat("Mean\t\t", sprintf("%.2f", xbar), "\n", sep="")
cat("Std_Dev\t\t", sprintf("%.2f", stddev), "\n", sep="")
} else {
  stop("The expected column 'Data' does not exist in the provided data.")
}
cat("*** DONE PROCESSING ***\n")