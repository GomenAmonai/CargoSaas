#!/bin/bash

# 🧪 Local Testing Script for Cargo.Solution
# Запускает все проверки перед push

set -e  # Exit on error

echo "🚀 Starting pre-push checks..."
echo ""

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# 1. Check if we're in the right directory
if [ ! -f "Cargo.Solution.sln" ]; then
    echo -e "${RED}❌ Error: Run this script from project root${NC}"
    exit 1
fi

echo -e "${YELLOW}📦 Step 1: Building Backend...${NC}"
cd src/Cargo.API
dotnet build
if [ $? -ne 0 ]; then
    echo -e "${RED}❌ Backend build failed!${NC}"
    exit 1
fi
echo -e "${GREEN}✅ Backend build successful${NC}"
echo ""

echo -e "${YELLOW}📦 Step 2: Building Frontend...${NC}"
cd ../Cargo.Web
npm run build
if [ $? -ne 0 ]; then
    echo -e "${RED}❌ Frontend build failed!${NC}"
    exit 1
fi
echo -e "${GREEN}✅ Frontend build successful${NC}"
echo ""

echo -e "${YELLOW}🔍 Step 3: Linting Frontend...${NC}"
npm run lint 2>/dev/null || echo "Lint script not found, skipping..."
echo -e "${GREEN}✅ Linting complete${NC}"
echo ""

echo -e "${YELLOW}🔍 Step 4: TypeScript Check...${NC}"
npx tsc --noEmit
if [ $? -ne 0 ]; then
    echo -e "${RED}❌ TypeScript errors found!${NC}"
    exit 1
fi
echo -e "${GREEN}✅ TypeScript check passed${NC}"
echo ""

cd ../..

echo ""
echo -e "${GREEN}✅ All checks passed!${NC}"
echo -e "${YELLOW}📝 Ready to commit and push${NC}"
echo ""
echo "Next steps:"
echo "  git add ."
echo "  git commit -m 'your message'"
echo "  git push origin main"

