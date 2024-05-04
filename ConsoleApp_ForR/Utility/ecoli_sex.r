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

# Ensure the 'SEX' column exists
if (!"Sex" %in% names(data)) {
  stop("The expected column 'Sex' does not exist in the provided CSV file.")
}

# Calculate frequency table for the 'Sex' column
sex_freq <- table(data$Sex)

# Handle case where no data is available or all data is NA
if (is.null(sex_freq) || length(sex_freq) == 0) {
  writeLines(paste("Error: No valid 'Sex' data found -", Sys.time(), ":"), "error_log.txt")
  stop("No valid 'Sex' data found in the file.")
}

# Calculate cumulative frequency
cum_freq <- cumsum(sex_freq)

# Calculate cumulative percentage frequency
total <- sum(sex_freq)
if (total > 0) {
    cum_percent <- (cum_freq / total) * 100
} else {
    writeLines(paste("Error: Total frequency is zero -", Sys.time(), ":"), "error_log.txt")
    stop("Total frequency count is zero, cannot calculate percentages.")
}

# Constants for Wilson Score Interval
z <- qnorm(0.975)  # 1.96 for 95% confidence
interval_parts <- sapply(sex_freq, function(x) {
    p_hat <- x / total
    denom <- 1 + z^2 / total
    center <- (p_hat + z^2 / (2 * total)) / denom
    margin <- (z * sqrt(p_hat * (1 - p_hat) / total + z^2 / (4 * total^2))) / denom
    c(lower = center - margin, upper = center + margin)
})

# Print the results in a table format using cat
cat("Frequency Table for Sex:\n")
cat("-------------------------------------------------------------------------\n")
cat(sprintf("%-10s %-12s %-12s %-12s %-12s %-12s\n", "Sex", "Frequency", "Cum Freq", "Cum %", "Wilson LCL", "Wilson UCL"))
cat("-------------------------------------------------------------------------\n")
names <- names(sex_freq)
for (i in seq_along(sex_freq)) {
    cat(sprintf("%-10s %-12d %-12d %-12s %-12.2f %-12.2f\n", names[i], sex_freq[i], cum_freq[i], paste0(format(round(cum_percent[i], 2), nsmall = 2), "%"), interval_parts[1, i] * 100, interval_parts[2, i] * 100))
}
cat("-------------------------------------------------------------------------\n")
cat("-------------------------------------------------------------------------\n")